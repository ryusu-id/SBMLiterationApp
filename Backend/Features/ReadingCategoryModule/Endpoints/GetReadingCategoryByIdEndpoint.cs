using FastEndpoints;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingCategoryModule.Domain;

namespace PureTCOWebApp.Features.ReadingCategoryModule.Endpoints;

public record GetReadingCategoryByIdRequest(int Id);

public record GetReadingCategoryByIdResponse(int Id, string CategoryName);

public class GetReadingCategoryByIdEndpoint(ApplicationDbContext dbContext)
    : Endpoint<GetReadingCategoryByIdRequest, ApiResponse<GetReadingCategoryByIdResponse>>
{
    public override void Configure()
    {
        Get("{id}");
        Group<ReadingCategoryEndpointGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetReadingCategoryByIdRequest req, CancellationToken ct)
    {
        var category = await dbContext.ReadingCategories.FindAsync([req.Id], ct);

        if (category is null)
        {
            var error = CrudDomainError.NotFound("ReadingCategory", req.Id);
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>((Result)error));
            return;
        }

        var response = new GetReadingCategoryByIdResponse(category.Id, category.CategoryName);

        await Send.OkAsync(Result.Success(response), cancellation: ct);
    }
}
