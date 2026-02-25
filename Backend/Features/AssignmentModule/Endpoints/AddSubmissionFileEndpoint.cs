using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.AssignmentModule.Domain;
using PureTCOWebApp.Features.FileSystem;

namespace PureTCOWebApp.Features.AssignmentModule.Endpoints;

public class AddSubmissionFileRequest
{
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public string? FileName { get; set; }
    public IFormFile? File { get; set; }
    public string? ExternalLink { get; set; }
}

public record AddSubmissionFileResponse(
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
        Post("{id}/submissions/{submissionId}/files");
        Group<AssignmentEndpointGroup>();
        Roles("participant");
        AllowFileUploads();
    }

    public override async Task HandleAsync(AddSubmissionFileRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);

        // Validate exactly one of file / external link
        bool hasFile = req.File is { Length: > 0 };
        bool hasExternalLink = !string.IsNullOrWhiteSpace(req.ExternalLink);

        if (hasFile == hasExternalLink)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)new Error("ExactlyOne", "Provide exactly one of 'file' or 'externalLink', not both or neither.")));
            return;
        }

        var submission = await dbContext.AssignmentSubmissions
            .FirstOrDefaultAsync(s => s.Id == req.SubmissionId && s.AssignmentId == req.Id, ct);

        if (submission is null)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)CrudDomainError.NotFound("Submission", req.SubmissionId)));
            return;
        }

        // Verify requester is a member of the submission's group
        var isMember = await dbContext.GroupMembers
            .AnyAsync(m => m.UserId == userId && m.GroupId == submission.GroupId, ct);

        if (!isMember)
        {
            await Send.ResultAsync(TypedResults.Forbid());
            return;
        }

        string? fileUri = null;
        string fileName;

        if (hasFile)
        {
            // Upload to MinIO
            var ext = Path.GetExtension(req.File!.FileName);
            var objectName = $"submissions/{req.SubmissionId}/{Guid.NewGuid()}{ext}";

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
            new AddSubmissionFileResponse(file.Id, file.FileName, file.FileUri, file.ExternalLink, file.UploadedAt)
        ), cancellation: ct);
    }
}
