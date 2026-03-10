using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.AssignmentModule.Endpoints.AssignmentEndpoints;

public record DeleteAssignmentRequest(int Id);

public class DeleteAssignmentEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<DeleteAssignmentRequest, ApiResponse>
{
    public override void Configure()
    {
        Delete("{id}");
        Group<AssignmentEndpointGroup>();
        Roles("admin");
    }

    public override async Task HandleAsync(DeleteAssignmentRequest req, CancellationToken ct)
    {
        var assignment = await dbContext.Assignments.FindAsync([req.Id], ct);

        if (assignment is null)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)CrudDomainError.NotFound("Assignment", req.Id)));
            return;
        }

        var hasBlockingSubmissions = await dbContext.AssignmentSubmissions
            .AnyAsync(s => s.AssignmentId == req.Id && (s.IsCompleted || s.Files.Any()), ct);

        if (hasBlockingSubmissions)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)new Error("CannotDelete", "Cannot delete assignment: it has completed or file-attached submissions.")));
            return;
        }

        dbContext.Assignments.Remove(assignment);
        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(), cancellation: ct);
    }
}
