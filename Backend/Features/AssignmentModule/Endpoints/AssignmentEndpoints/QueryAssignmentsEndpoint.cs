using FastEndpoints;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.AssignmentModule.Domain;

namespace PureTCOWebApp.Features.AssignmentModule.Endpoints.AssignmentEndpoints;

public record QueryAssignmentsRequest(string? Title = null) : PagingQuery;

public record AssignmentListItem(int Id, string Title, string? Description, DateTime? DueDate, DateTime CreateTime);

public class QueryAssignmentsEndpoint(ApplicationDbContext dbContext)
    : Endpoint<QueryAssignmentsRequest, PagingResult<AssignmentListItem>>
{
    public override void Configure()
    {
        Get("");
        Group<AssignmentEndpointGroup>();
        Roles("admin");
    }

    public override async Task HandleAsync(QueryAssignmentsRequest req, CancellationToken ct)
    {
        var query = dbContext.Assignments.AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Title))
        {
            query = query.Where(a => a.Title.Contains(req.Title));
        }

        var result = await PagingService.PaginateQueryAsync<Assignment, AssignmentListItem>(
            query,
            req,
            a => new AssignmentListItem(a.Id, a.Title, a.Description, a.DueDate, a.CreateTime),
            ct);

        await Send.OkAsync(result, ct);
    }
}
