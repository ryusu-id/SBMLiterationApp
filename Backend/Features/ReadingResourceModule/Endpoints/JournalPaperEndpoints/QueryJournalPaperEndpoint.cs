using FastEndpoints;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain;

namespace PureTCOWebApp.Features.ReadingResourceModule.Endpoints.JournalPaperEndpoints;

public record QueryJournalPaperRequest(
    string? Title = null,
    string? ISBN = null,
    string? ReadingCategory = null,
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
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        
        var query = dbContext.JournalPapers.Where(x => x.UserId == userId);

        var predicate = PredicateBuilder.True<JournalPaper>();

        if (!string.IsNullOrWhiteSpace(req.Title))
        {
            predicate = predicate.And(x => x.Title.Contains(req.Title));
        }

        if (!string.IsNullOrWhiteSpace(req.ISBN))
        {
            predicate = predicate.And(x => x.ISBN.Contains(req.ISBN));
        }

        if (!string.IsNullOrWhiteSpace(req.ReadingCategory))
        {
            predicate = predicate.And(x => x.ReadingCategory.Contains(req.ReadingCategory));
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