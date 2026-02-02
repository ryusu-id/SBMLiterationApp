using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.DailyReadsModule.Domain;

namespace PureTCOWebApp.Features.DailyReadsModule.Endpoints.DailyReadEndpoints;

public record QueryDailyReadsRequest(
    string? Title = null,
    string? Category = null,
    DateOnly? Date = null
) : PagingQuery;

public class QueryDailyReadsEndpoint(ApplicationDbContext dbContext)
    : Endpoint<QueryDailyReadsRequest, PagingResult<DailyRead>>
{
    public override void Configure()
    {
        Get("");
        Group<DailyReadsEndpointGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(QueryDailyReadsRequest req, CancellationToken ct)
    {
        var query = dbContext.DailyReads.AsQueryable();

        var predicate = PredicateBuilder.True<DailyRead>();

        if (!string.IsNullOrWhiteSpace(req.Title))
        {
            predicate = predicate.And(x => x.Title.Contains(req.Title));
        }

        if (!string.IsNullOrWhiteSpace(req.Category))
        {
            predicate = predicate.And(x => x.Category != null && x.Category.Contains(req.Category));
        }

        if (req.Date.HasValue)
        {
            predicate = predicate.And(x => x.Date == req.Date.Value);
        }

        query = query.Where(predicate).OrderByDescending(x => x.Date);

        var result = await PagingService.PaginateQueryAsync(query, req, dbContext, ct);

        await Send.OkAsync(result, ct);
    }
}
