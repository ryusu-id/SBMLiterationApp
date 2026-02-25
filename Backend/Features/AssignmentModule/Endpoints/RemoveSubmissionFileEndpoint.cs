using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.AssignmentModule.Endpoints;

public record RemoveSubmissionFileRequest(int Id, int SubmissionId, int FileId);

public class RemoveSubmissionFileEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<RemoveSubmissionFileRequest, ApiResponse>
{
    public override void Configure()
    {
        Delete("{id}/submissions/{submissionId}/files/{fileId}");
        Group<AssignmentEndpointGroup>();
        Roles("participant");
    }

    public override async Task HandleAsync(RemoveSubmissionFileRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);

        var file = await dbContext.AssignmentSubmissionFiles
            .Include(f => f.Submission)
            .FirstOrDefaultAsync(f =>
                f.Id == req.FileId &&
                f.AssignmentSubmissionId == req.SubmissionId &&
                f.Submission.AssignmentId == req.Id, ct);

        if (file is null)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)CrudDomainError.NotFound("SubmissionFile", req.FileId)));
            return;
        }

        // Verify requester is a member of the submission's group
        var isMember = await dbContext.GroupMembers
            .AnyAsync(m => m.UserId == userId && m.GroupId == file.Submission.GroupId, ct);

        if (!isMember)
        {
            await Send.ResultAsync(TypedResults.Forbid());
            return;
        }

        dbContext.AssignmentSubmissionFiles.Remove(file);
        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(), cancellation: ct);
    }
}
