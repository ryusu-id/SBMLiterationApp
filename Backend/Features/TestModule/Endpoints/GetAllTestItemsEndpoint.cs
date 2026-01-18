using FastEndpoints;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.TestModule.Endpoints;


public record GetTestItemRequest(
    string? Name = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? IsActive = null
) : PagingQuery;

public class GetTestItemEndpoint(ApplicationDbContext dbContext)
    : Endpoint<GetTestItemRequest, PagingResult<TestItem>>
{
    public override void Configure()
    {
        Get("");
        Group<TestModuleEndpointGroup>();
    }

    public override async Task HandleAsync(GetTestItemRequest req, CancellationToken ct)
    {
        var query = dbContext.TestItems.AsQueryable();

        var predicate = PredicateBuilder.True<TestItem>();

        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            predicate = predicate.And(x =>
                x.Name.Contains(req.Search) ||
                (x.Description != null && x.Description.Contains(req.Search))
            );
        }

        if (!string.IsNullOrWhiteSpace(req.Name))
        {
            predicate = predicate.And(x => x.Name.Contains(req.Name));
        }

        if (req.MinPrice.HasValue)
        {
            predicate = predicate.And(x => x.Price >= req.MinPrice.Value);
        }

        if (req.MaxPrice.HasValue)
        {
            predicate = predicate.And(x => x.Price <= req.MaxPrice.Value);
        }

        if (req.IsActive.HasValue)
        {
            predicate = predicate.And(x => x.IsActive == req.IsActive.Value);
        }

        query = query.Where(predicate);

        var result = await PagingService.PaginateQueryAsync(query, req, dbContext, ct);

        await Send.OkAsync(result, cancellation: ct);
    }
}