using FastEndpoints;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.TestModule.Endpoints;

public record GetByIdTestItemRequest(int Id);

public record GetByIdTestItemResponse(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    bool IsActive
);

public class GetTestItemByIdEndpoint(ApplicationDbContext _dbContext) : Endpoint<GetByIdTestItemRequest, ApiResponse<GetByIdTestItemResponse>>
{
    public override void Configure()
    {
        Get("{id}");
        Group<TestModuleEndpointGroup>();
    }

    public override async Task HandleAsync(GetByIdTestItemRequest req, CancellationToken ct)
    {
        var testItem = await _dbContext.TestItems.FindAsync([req.Id], ct);

        if (testItem is null)
        {
            var error = CrudDomainError.NotFound(nameof(TestItem), req.Id);
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>((Result)error));
            return;
        }

        var response = new GetByIdTestItemResponse(
            testItem.Id,
            testItem.Name,
            testItem.Description,
            testItem.Price,
            testItem.IsActive
        );

        await Send.OkAsync(Result.Success(response), cancellation: ct);
    }
}