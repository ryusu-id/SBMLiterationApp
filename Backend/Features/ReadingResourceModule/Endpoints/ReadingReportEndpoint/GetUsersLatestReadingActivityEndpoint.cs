using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain;

namespace PureTCOWebApp.Features.ReadingResourceModule.Endpoints.ReadingReportEndpoint;

public record GetUsersLatestReadingActivityRequest(
    int UserId
) : PagingQuery;

public record UserLatestReadingActivity(
    int UserId,
    int ReadingResourceId,
    string ResourceTitle,
    string ResourceType,
    DateTime LastReportDate,
    int CurrentPage,
    string LastInsight,
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
        if (req.UserId <= 0)
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        req = req with { SortBy = string.IsNullOrEmpty(req.SortBy) ? "-ReportDate" : req.SortBy };

        var latestReports = from report in context.ReadingReports
                            where report.UserId == req.UserId
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
                        ResourceType = resource is Book ? "Book" : "JournalPaper",
                        report.ReportDate,
                        report.CurrentPage,
                        report.Insight,
                        resource.CoverImageUri
                    };

        query = query.AsNoTracking();

        var mappedQuery = query.Select(x => new UserLatestReadingActivity(
            x.UserId,
            x.ReadingResourceId,
            x.Title,
            x.ResourceType,
            x.ReportDate,
            x.CurrentPage,
            x.Insight,
            x.CoverImageUri
        ));

        var result = await PagingService.PaginateQueryAsync(
            mappedQuery,
            req,
            ct);

        await Send.OkAsync(result, ct);
    }
}