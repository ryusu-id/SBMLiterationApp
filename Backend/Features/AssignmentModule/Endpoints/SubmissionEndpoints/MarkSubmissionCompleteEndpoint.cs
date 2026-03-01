using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.AssignmentModule.Endpoints.SubmissionEndpoints;

public record MarkSubmissionCompleteRequest(int AssignmentId);

public class MarkSubmissionCompleteEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<MarkSubmissionCompleteRequest, ApiResponse>
{
    public override void Configure()
    {
        Post("{assignmentId}/submission/my/complete");
        Group<AssignmentEndpointGroup>();
        Roles("participant");
    }

    public override async Task HandleAsync(MarkSubmissionCompleteRequest req, CancellationToken ct)
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

        var submission = await dbContext.AssignmentSubmissions
            .FirstOrDefaultAsync(s => s.AssignmentId == req.AssignmentId && s.GroupId == groupId, ct);

        if (submission is null)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)CrudDomainError.NotFound("Submission", req.AssignmentId)));
            return;
        }

        submission.MarkAsComplete();

        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(), cancellation: ct);
    }
}
