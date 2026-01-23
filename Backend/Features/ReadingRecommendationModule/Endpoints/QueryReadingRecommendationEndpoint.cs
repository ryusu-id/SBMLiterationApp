using FastEndpoints;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingRecommendationModule.Domain;

namespace PureTCOWebApp.Features.ReadingRecommendationModule.Endpoints;

public record QueryReadingRecommendationRequest(
    string? Title = null,
    string? Category = null,
    string? Author = null
) : PagingQuery;

public class QueryReadingRecommendationEndpoint(ApplicationDbContext dbContext)
    : Endpoint<QueryReadingRecommendationRequest, PagingResult<ReadingRecommendation>>
{
    public override void Configure()
    {
        Get("");
        Group<ReadingRecommendationEndpointGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(QueryReadingRecommendationRequest req, CancellationToken ct)
    {
        var query = dbContext.ReadingRecommendations.AsQueryable();

        var predicate = PredicateBuilder.True<ReadingRecommendation>();

        if (!string.IsNullOrWhiteSpace(req.Title))
        {
            predicate = predicate.And(x => x.Title.Contains(req.Title));
        }

        if (!string.IsNullOrWhiteSpace(req.Category))
        {
            predicate = predicate.And(x => x.ReadingCategory.Contains(req.Category));
        }

        if (!string.IsNullOrWhiteSpace(req.Author))
        {
            predicate = predicate.And(x => x.Authors.Contains(req.Author));
        }

        query = query.Where(predicate);

        var result = await PagingService.PaginateQueryAsync(query, req, dbContext, ct);

        await Send.OkAsync(result, ct);
    }
}
