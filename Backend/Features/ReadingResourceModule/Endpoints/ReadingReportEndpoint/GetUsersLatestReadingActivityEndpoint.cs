using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain;

namespace PureTCOWebApp.Features.ReadingResourceModule.Endpoints.ReadingReportEndpoint;

public record GetUsersLatestReadingActivityRequest() : PagingQuery;

public record UserLatestReadingActivity(
    int ReadingResourceId,
    string ResourceTitle,
    string ResourceType,
    DateTime ReportDate,
    int CurrentPage,
    string Insight,
    int TimeSpent,
    string? CoverImageUri);

public class GetUsersLatestReadingActivityEndpoint(ApplicationDbContext context)
    : Endpoint<GetUsersLatestReadingActivityRequest, PagingResult<UserLatestReadingActivity>>
{
    public override void Configure()
    {
        Get("/reports/latest-activity");
        Group<ReadingResourceEndpointGroup>();
    }

    public override async Task HandleAsync(GetUsersLatestReadingActivityRequest req, CancellationToken ct)
    {
        if (!int.TryParse(User.Identities.FirstOrDefault()?.FindFirst("sub")?.Value, out var userId))
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        req = req with { SortBy = string.IsNullOrEmpty(req.SortBy) ? "-ReportDate" : req.SortBy };

        var latestReports = from report in context.ReadingReports
                            where report.UserId == userId
                            group report by new { report.UserId, report.ReadingResourceId } into g
                            select new
                            {
                                g.Key.UserId,
                                g.Key.ReadingResourceId,
                                LatestReportDate = g.Max(r => r.ReportDate)
                            };

        var query = from latest in latestReports
                    join report in context.ReadingReports on new { latest.UserId, latest.ReadingResourceId, ReportDate = latest.LatestReportDate }
                        equals new { report.UserId, report.ReadingResourceId, report.ReportDate }
                    join resource in context.Set<ReadingResourceBase>() on report.ReadingResourceId equals resource.Id
                    select new
                    {
                        report.UserId,
                        report.ReadingResourceId,
                        resource.Title,
                        Resource = resource,
                        report.ReportDate,
                        report.CurrentPage,
                        report.Insight,
                        resource.CoverImageUri,
                        report.TimeSpent
                    };

        query = query.AsNoTracking();

        var result = await PagingService.PaginateQueryAsync(
            query,
            req,
            ct);

        var mappedResult = new PagingResult<UserLatestReadingActivity>(
            result.Rows.Select(x => new UserLatestReadingActivity(
                x.ReadingResourceId,
                x.Title,
                x.Resource is Book ? "Book" : "JournalPaper",
                x.ReportDate,
                x.CurrentPage,
                x.Insight,
                x.TimeSpent,
                x.CoverImageUri
            )).ToList(),
            result.Page,
            result.RowsPerPage,
            result.TotalRows, result.TotalPages
        );

        await Send.OkAsync(mappedResult, ct);
    }
}