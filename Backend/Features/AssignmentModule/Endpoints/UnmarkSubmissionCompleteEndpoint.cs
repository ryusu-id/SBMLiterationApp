using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.AssignmentModule.Endpoints;

public record UnmarkSubmissionCompleteRequest(int Id, int SubmissionId);

public class UnmarkSubmissionCompleteEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<UnmarkSubmissionCompleteRequest, ApiResponse>
{
    public override void Configure()
    {
        Delete("{id}/submissions/{submissionId}/complete");
        Group<AssignmentEndpointGroup>();
        Roles("participant");
    }

    public override async Task HandleAsync(UnmarkSubmissionCompleteRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);

        var submission = await dbContext.AssignmentSubmissions
            .FirstOrDefaultAsync(s => s.Id == req.SubmissionId && s.AssignmentId == req.Id, ct);

        if (submission is null)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)CrudDomainError.NotFound("Submission", req.SubmissionId)));
            return;
        }

        var isMember = await dbContext.GroupMembers
            .AnyAsync(m => m.UserId == userId && m.GroupId == submission.GroupId, ct);

        if (!isMember)
        {
            await Send.ResultAsync(TypedResults.Forbid());
            return;
        }

        submission.MarkAsIncomplete();

        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(), cancellation: ct);
    }
}
