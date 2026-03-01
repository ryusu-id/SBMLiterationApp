using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.AssignmentModule.Domain;
using PureTCOWebApp.Features.FileSystem;

namespace PureTCOWebApp.Features.AssignmentModule.Endpoints.SubmissionEndpoints;

public class AddSubmissionFileRequest
{
    public int AssignmentId { get; set; }
    public string? FileName { get; set; }
    public IFormFile? File { get; set; }
    public string? ExternalLink { get; set; }
}

public record AddSubmissionFileResponse(
    int SubmissionId,
    int FileId,
    string FileName,
    string? FileUri,
    string? ExternalLink,
    DateTime UploadedAt);

public class AddSubmissionFileEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork,
    IMinIOService minioService
) : Endpoint<AddSubmissionFileRequest, ApiResponse<AddSubmissionFileResponse>>
{
    public override void Configure()
    {
        Post("{assignmentId}/submission/my/files");
        Group<AssignmentEndpointGroup>();
        Roles("participant");
        AllowFileUploads();
    }

    public override async Task HandleAsync(AddSubmissionFileRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);

        bool hasFile = req.File is { Length: > 0 };
        bool hasExternalLink = !string.IsNullOrWhiteSpace(req.ExternalLink);

        if (hasFile == hasExternalLink)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)new Error("ExactlyOne", "Provide exactly one of 'file' or 'externalLink', not both or neither.")));
            return;
        }

        var assignmentExists = await dbContext.Assignments
            .AsNoTracking()
            .AnyAsync(a => a.Id == req.AssignmentId, ct);

        if (!assignmentExists)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)CrudDomainError.NotFound("Assignment", req.AssignmentId)));
            return;
        }

        var groupId = await dbContext.GroupMembers
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Select(m => m.GroupId)
            .FirstOrDefaultAsync(ct);

        if (groupId == default)
        {
            await Send.ResultAsync(TypedResults.Forbid());
            return;
        }

        var submission = await dbContext.AssignmentSubmissions
            .FirstOrDefaultAsync(s => s.AssignmentId == req.AssignmentId && s.GroupId == groupId, ct);

        if (submission is null)
        {
            submission = AssignmentSubmission.Create(req.AssignmentId, groupId);
            await dbContext.AssignmentSubmissions.AddAsync(submission, ct);
            var createResult = await unitOfWork.SaveChangesAsync(ct);

            if (createResult.IsFailure)
            {
                await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(createResult));
                return;
            }
        }

        string? fileUri = null;
        string fileName;

        if (hasFile)
        {
            var ext = Path.GetExtension(req.File!.FileName);
            var objectName = $"submissions/{submission.Id}/{Guid.NewGuid()}{ext}";

            using var stream = req.File.OpenReadStream();
            fileUri = await minioService.UploadFileAsync(stream, objectName, req.File.ContentType, ct);
            fileName = string.IsNullOrWhiteSpace(req.FileName) ? req.File.FileName : req.FileName;
        }
        else
        {
            fileName = string.IsNullOrWhiteSpace(req.FileName) ? req.ExternalLink! : req.FileName;
        }

        var file = AssignmentSubmissionFile.Create(
            submission.Id,
            userId,
            fileName,
            fileUri,
            hasExternalLink ? req.ExternalLink : null);

        await dbContext.AssignmentSubmissionFiles.AddAsync(file, ct);
        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(
            new AddSubmissionFileResponse(submission.Id, file.Id, file.FileName, file.FileUri, file.ExternalLink, file.CreateTime)
        ), cancellation: ct);
    }
}
