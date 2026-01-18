using FastEndpoints;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain;

namespace PureTCOWebApp.Features.ReadingResourceModule.Endpoints;

public record QueryJournalPaperRequest(
    string? Title = null,
    string? ISBN = null,
    string? BookCategory = null,
    string? Authors = null,
    string? PublishYear = null
) : PagingQuery;

public class QueryJournalPaperEndpoint(ApplicationDbContext dbContext)
    : Endpoint<QueryJournalPaperRequest, PagingResult<JournalPaper>>
{
    public override void Configure()
    {
        Get("journals");
        Group<ReadingResourceEndpointGroup>();
    }

    public override async Task HandleAsync(QueryJournalPaperRequest req, CancellationToken ct)
    {
        var query = dbContext.JournalPapers.AsQueryable();

        var predicate = PredicateBuilder.True<JournalPaper>();

        if (!string.IsNullOrWhiteSpace(req.Title))
        {
            predicate = predicate.And(x => x.Title.Contains(req.Title));
        }

        if (!string.IsNullOrWhiteSpace(req.ISBN))
        {
            predicate = predicate.And(x => x.ISBN.Contains(req.ISBN));
        }

        if (!string.IsNullOrWhiteSpace(req.BookCategory))
        {
            predicate = predicate.And(x => x.BookCategory.Contains(req.BookCategory));
        }

        if (!string.IsNullOrWhiteSpace(req.Authors))
        {
            predicate = predicate.And(x => x.Authors.Contains(req.Authors));
        }

        if (!string.IsNullOrWhiteSpace(req.PublishYear))
        {
            predicate = predicate.And(x => x.PublishYear.Contains(req.PublishYear));
        }
        
        query = query.Where(predicate);

        var result = await PagingService.PaginateQueryAsync(query, req, dbContext, ct);

        await Send.OkAsync(result, cancellation: ct);
    }
}