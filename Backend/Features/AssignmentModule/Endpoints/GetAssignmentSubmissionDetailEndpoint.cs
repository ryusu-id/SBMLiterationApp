using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.AssignmentModule.Endpoints;

public record GetAssignmentSubmissionDetailRequest(int Id, int SubmissionId);

public record SubmissionFileDetail(
    int FileId,
    string FileName,
    string? FileUri,
    string? ExternalLink,
    int UploadedByUserId,
    string UploadedByName,
    DateTime UploadedAt);

public record SubmissionDetail(
    int SubmissionId,
    int AssignmentId,
    int GroupId,
    string GroupName,
    bool IsCompleted,
    DateTime? CompletedAt,
    DateTime CreateTime,
    List<SubmissionFileDetail> Files);

public class GetAssignmentSubmissionDetailEndpoint(ApplicationDbContext dbContext)
    : Endpoint<GetAssignmentSubmissionDetailRequest, ApiResponse<SubmissionDetail>>
{
    public override void Configure()
    {
        Get("{id}/submissions/{submissionId}");
        Group<AssignmentEndpointGroup>();
        Roles("admin");
    }

    public override async Task HandleAsync(GetAssignmentSubmissionDetailRequest req, CancellationToken ct)
    {
        var submission = await dbContext.AssignmentSubmissions
            .Include(s => s.Group)
            .Include(s => s.Files)
                .ThenInclude(f => f.UploadedBy)
            .FirstOrDefaultAsync(s => s.Id == req.SubmissionId && s.AssignmentId == req.Id, ct);

        if (submission is null)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)CrudDomainError.NotFound("Submission", req.SubmissionId)));
            return;
        }

        var files = submission.Files.Select(f => new SubmissionFileDetail(
            f.Id, f.FileName, f.FileUri, f.ExternalLink,
            f.UploadedByUserId, f.UploadedBy.Fullname, f.UploadedAt)).ToList();

        var response = new SubmissionDetail(
            submission.Id,
            submission.AssignmentId,
            submission.GroupId,
            submission.Group.Name,
            submission.IsCompleted,
            submission.CompletedAt,
            submission.CreateTime,
            files);

        await Send.OkAsync(Result.Success(response), cancellation: ct);
    }
}
