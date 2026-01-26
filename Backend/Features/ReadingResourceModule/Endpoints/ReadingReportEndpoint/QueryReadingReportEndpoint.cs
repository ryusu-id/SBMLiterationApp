using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain.Entities;

namespace PureTCOWebApp.Features.ReadingResourceModule.Endpoints.ReadingReportEndpoint;

public record QueryReadingReportRequest(
    int? ReadingResourceId = null
) : PagingQuery;

public record QueryReadingReportResponse(
    int Id,
    int UserId,
    int ReadingResourceId,
    DateTime ReportDate,
    int CurrentPage,
    string Insight,
    int TimeSpent,
    string? CoverImageUri
);

public class QueryReadingReportEndpoint(ApplicationDbContext context)
    : Endpoint<QueryReadingReportRequest, PagingResult<QueryReadingReportResponse>>
{
    public override void Configure()
    {
        Get("/reports");
        Group<ReadingResourceEndpointGroup>();
    }

    public override async Task HandleAsync(QueryReadingReportRequest req, CancellationToken ct)
    {
        req = req with { SortBy = string.IsNullOrEmpty(req.SortBy) ? "Id" : req.SortBy };
        var userId = int.Parse(User.FindFirst("sub")!.Value);

        var query = context.ReadingReports
            .Include(x => x.ReadingResource)
            .Where(x => x.ReadingResource.UserId == userId);
        
        var predicate = PredicateBuilder.True<ReadingReport>();

        if (req.ReadingResourceId.HasValue)
            predicate = predicate.And(x => x.ReadingResourceId == req.ReadingResourceId.Value);

        query = query.Where(predicate).AsNoTracking();

        var result = await PagingService.PaginateQueryAsync(query, req, report => new QueryReadingReportResponse(
            report.Id,
            report.UserId,
            report.ReadingResourceId,
            report.ReportDate,
            report.CurrentPage,
            report.Insight,
            report.TimeSpent,
            report.ReadingResource.CoverImageUri
        ), ct);

        await Send.OkAsync(result, ct);
    }
}