using FastEndpoints;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain;

namespace PureTCOWebApp.Features.ReadingResourceModule.Endpoints;

public record QueryBookRequest(
    string? Title = null,
    string? ISBN = null,
    string? BookCategory = null,
    string? Authors = null,
    string? PublishYear = null
) : PagingQuery;

public class QueryBookEndpoint(ApplicationDbContext dbContext)
    : Endpoint<QueryBookRequest, PagingResult<Book>>
{
    public override void Configure()
    {
        Get("books");
        Group<ReadingResourceEndpointGroup>();
    }

    public override async Task HandleAsync(QueryBookRequest req, CancellationToken ct)
    {
        var query = dbContext.Books.AsQueryable();

        var predicate = PredicateBuilder.True<Book>();

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