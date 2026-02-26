using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.AssignmentModule.Domain;

namespace PureTCOWebApp.Features.AssignmentModule.Endpoints.SubmissionEndpoints;

public record QueryAssignmentSubmissionsRequest(int Id) : PagingQuery;

public record SubmissionListItem(
    int? SubmissionId,
    int GroupId,
    string GroupName,
    bool IsCompleted,
    DateTime? CompletedAt,
    int FileCount,
    DateTime? CreateTime);

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
        var query = from g in dbContext.Groups
                    join s in dbContext.AssignmentSubmissions
                        .Include(s => s.Files)
                        .Where(s => s.AssignmentId == req.Id)
                    on g.Id equals s.GroupId into submissions
                    from sub in submissions.DefaultIfEmpty()
                    select new { Group = g, Submission = sub };

        var result = await PagingService.PaginateQueryAsync(
            query,
            req,
            x => new SubmissionListItem(
                x.Submission?.Id,
                x.Group.Id,
                x.Group.Name,
                x.Submission?.IsCompleted ?? false,
                x.Submission?.CompletedAt,
                x.Submission?.Files.Count ?? 0,
                x.Submission?.CreateTime),
            ct);

        await Send.OkAsync(result, ct);
    }
}
