using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.AssignmentModule.Endpoints.SubmissionEndpoints;

public record RemoveSubmissionFileRequest(int AssignmentId, int FileId);

public class RemoveSubmissionFileEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<RemoveSubmissionFileRequest, ApiResponse>
{
    public override void Configure()
    {
        Delete("{assignmentId}/submission/my/files/{fileId}");
        Group<AssignmentEndpointGroup>();
        Roles("participant");
    }

    public override async Task HandleAsync(RemoveSubmissionFileRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);

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

        var file = await dbContext.AssignmentSubmissionFiles
            .Include(f => f.Submission)
            .FirstOrDefaultAsync(f =>
                f.Id == req.FileId &&
                f.Submission.AssignmentId == req.AssignmentId &&
                f.Submission.GroupId == groupId, ct);

        if (file is null)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)CrudDomainError.NotFound("SubmissionFile", req.FileId)));
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
