using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingCategoryModule.Domain;

namespace PureTCOWebApp.Features.ReadingCategoryModule.Endpoints;

public record QueryReadingCategoryRequest(string? CategoryName = null) : PagingQuery;

public class QueryReadingCategoryEndpoint(ApplicationDbContext dbContext)
    : Endpoint<QueryReadingCategoryRequest, PagingResult<ReadingCategory>>
{
    public override void Configure()
    {
        Get("");
        Group<ReadingCategoryEndpointGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(QueryReadingCategoryRequest req, CancellationToken ct)
    {
        var query = dbContext.ReadingCategories.AsQueryable();

        var predicate = PredicateBuilder.True<ReadingCategory>();

        if (!string.IsNullOrWhiteSpace(req.CategoryName))
        {
            predicate = predicate.And(x => x.CategoryName.Contains(req.CategoryName));
        }

        query = query.Where(predicate);

        var result = await PagingService.PaginateQueryAsync(query, req, dbContext, ct);

        await Send.OkAsync(result, ct);
    }
}