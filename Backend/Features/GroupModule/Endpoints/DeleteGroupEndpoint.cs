using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.GroupModule.Endpoints;

public record DeleteGroupRequest(int Id);

public class DeleteGroupEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<DeleteGroupRequest, ApiResponse>
{
    public override void Configure()
    {
        Delete("{id}");
        Group<GroupEndpointGroup>();
        Roles("admin");
    }

    public override async Task HandleAsync(DeleteGroupRequest req, CancellationToken ct)
    {
        var group = await dbContext.Groups.FindAsync([req.Id], ct);

        if (group is null)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)CrudDomainError.NotFound("Group", req.Id)));
            return;
        }

        var hasBlockingSubmissions = await dbContext.AssignmentSubmissions
            .AnyAsync(s => s.GroupId == req.Id && (s.IsCompleted || s.Files.Any()), ct);

        if (hasBlockingSubmissions)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)new Error("CannotDelete", "Cannot delete group: it has completed or file-attached submissions.")));
            return;
        }

        var cleanSubmissions = await dbContext.AssignmentSubmissions
            .Where(s => s.GroupId == req.Id)
            .ToListAsync(ct);

        if (cleanSubmissions.Count > 0)
            dbContext.AssignmentSubmissions.RemoveRange(cleanSubmissions);

        dbContext.Groups.Remove(group);
        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(), cancellation: ct);
    }
}
