using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.AssignmentModule.Endpoints;

public record GetAssignmentDetailRequest(int Id);

public record AssignmentDetail(
    int Id,
    string Title,
    string? Description,
    DateTime? DueDate,
    DateTime CreateTime,
    DateTime? UpdateTime);

public class GetAssignmentDetailEndpoint(ApplicationDbContext dbContext)
    : Endpoint<GetAssignmentDetailRequest, ApiResponse<AssignmentDetail>>
{
    public override void Configure()
    {
        Get("{id}");
        Group<AssignmentEndpointGroup>();
        Roles("admin", "participant");
    }

    public override async Task HandleAsync(GetAssignmentDetailRequest req, CancellationToken ct)
    {
        var assignment = await dbContext.Assignments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == req.Id, ct);

        if (assignment is null)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)CrudDomainError.NotFound("Assignment", req.Id)));
            return;
        }

        var response = new AssignmentDetail(
            assignment.Id,
            assignment.Title,
            assignment.Description,
            assignment.DueDate,
            assignment.CreateTime,
            assignment.UpdateTime);

        await Send.OkAsync(Result.Success(response), cancellation: ct);
    }
}
