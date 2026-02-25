using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.AssignmentModule.Domain;

namespace PureTCOWebApp.Features.AssignmentModule.Endpoints;

public record QueryAssignmentSubmissionsRequest(int Id) : PagingQuery;

public record SubmissionListItem(
    int SubmissionId,
    int GroupId,
    string GroupName,
    bool IsCompleted,
    DateTime? CompletedAt,
    int FileCount,
    DateTime CreateTime);

public class QueryAssignmentSubmissionsEndpoint(ApplicationDbContext dbContext)
    : Endpoint<QueryAssignmentSubmissionsRequest, PagingResult<SubmissionListItem>>
{
    public override void Configure()
    {
        Get("{id}/submissions");
        Group<AssignmentEndpointGroup>();
        Roles("admin");
    }

    public override async Task HandleAsync(QueryAssignmentSubmissionsRequest req, CancellationToken ct)
    {
        var query = dbContext.AssignmentSubmissions
            .Include(s => s.Group)
            .Include(s => s.Files)
            .Where(s => s.AssignmentId == req.Id);

        // Sort on the entity (AssignmentSubmission.CreateTime) before mapping.
        var result = await PagingService.PaginateQueryAsync<AssignmentSubmission, SubmissionListItem>(
            query,
            req,
            s => new SubmissionListItem(
                s.Id,
                s.GroupId,
                s.Group.Name,
                s.IsCompleted,
                s.CompletedAt,
                s.Files.Count,
                s.CreateTime),
            ct);

        await Send.OkAsync(result, ct);
    }
}
