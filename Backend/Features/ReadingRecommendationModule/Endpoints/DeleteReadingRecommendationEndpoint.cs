using FastEndpoints;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.ReadingRecommendationModule.Endpoints;

public record DeleteReadingRecommendationRequest(int Id);

public class DeleteReadingRecommendationEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<DeleteReadingRecommendationRequest, ApiResponse>
{
    public override void Configure()
    {
        Delete("{id}");
        Group<ReadingRecommendationEndpointGroup>();
    }

    public override async Task HandleAsync(DeleteReadingRecommendationRequest req, CancellationToken ct)
    {
        var recommendation = await dbContext.ReadingRecommendations.FindAsync([req.Id], ct);

        if (recommendation is null)
        {
            var error = CrudDomainError.NotFound("ReadingRecommendation", req.Id);
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>((Result)error));
            return;
        }

        dbContext.Remove(recommendation);
        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(), cancellation: ct);
    }
}
