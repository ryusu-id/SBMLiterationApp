using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.AssignmentModule.Endpoints;

public record MarkSubmissionCompleteRequest(int Id, int SubmissionId);

public class MarkSubmissionCompleteEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<MarkSubmissionCompleteRequest, ApiResponse>
{
    public override void Configure()
    {
        Post("{id}/submissions/{submissionId}/complete");
        Group<AssignmentEndpointGroup>();
        Roles("participant");
    }

    public override async Task HandleAsync(MarkSubmissionCompleteRequest req, CancellationToken ct)
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
