using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.TestModule;

namespace PureTCOWebApp.Features.TestModule.Endpoints;

public record DeleteTestItemRequest(int Id);

public class DeleteTestItemEndpoint(
    ApplicationDbContext _dbContext,
    UnitOfWork _unitOfWork
) : Endpoint<DeleteTestItemRequest, ApiResponse>
{
    public override void Configure()
    {
        Delete("{id}");
        Group<TestModuleEndpointGroup>();
    }

    public override async Task HandleAsync(DeleteTestItemRequest req, CancellationToken ct)
    {
        var testItem = await _dbContext.Set<TestItem>()
            .FirstOrDefaultAsync(x => x.Id == req.Id, ct);

        if (testItem is null)
        {
            var error = CrudDomainError.NotFound("TestItem", req.Id);
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>((Result)error));
            return;
        }

        _dbContext.Remove(testItem);
        var result = await _unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(), cancellation: ct);
    }
}