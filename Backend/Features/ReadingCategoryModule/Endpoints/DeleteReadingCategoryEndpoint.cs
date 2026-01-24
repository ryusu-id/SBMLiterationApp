using FastEndpoints;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.ReadingCategoryModule.Endpoints;

public record DeleteReadingCategoryRequest(int Id);

public class DeleteReadingCategoryEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<DeleteReadingCategoryRequest, ApiResponse>
{
    public override void Configure()
    {
        Delete("{id}");
        Group<ReadingCategoryEndpointGroup>();
    }

    public override async Task HandleAsync(DeleteReadingCategoryRequest req, CancellationToken ct)
    {
        var category = await dbContext.ReadingCategories.FindAsync([req.Id], ct);

        if (category is null)
        {
            var error = CrudDomainError.NotFound("ReadingCategory", req.Id);
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>((Result)error));
            return;
        }

        dbContext.Remove(category);
        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(), cancellation: ct);
    }
}
