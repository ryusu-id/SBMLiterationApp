using FastEndpoints;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.ReadingRecommendationModule.Endpoints;

public record GetReadingRecommendationByIdRequest(int Id);

public record GetReadingRecommendationByIdResponse(
    int Id,
    string Title,
    string ISBN,
    string ReadingCategory,
    string Authors,
    string PublishYear,
    int Page,
    string? ResourceLink,
    string? CoverImageUri
);

public class GetReadingRecommendationByIdEndpoint(ApplicationDbContext dbContext)
    : Endpoint<GetReadingRecommendationByIdRequest, ApiResponse<GetReadingRecommendationByIdResponse>>
{
    public override void Configure()
    {
        Get("{id}");
        Group<ReadingRecommendationEndpointGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetReadingRecommendationByIdRequest req, CancellationToken ct)
    {
        var recommendation = await dbContext.ReadingRecommendations.FindAsync([req.Id], ct);

        if (recommendation is null)
        {
            var error = CrudDomainError.NotFound("ReadingRecommendation", req.Id);
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>((Result)error));
            return;
        }

        var response = new GetReadingRecommendationByIdResponse(
            recommendation.Id,
            recommendation.Title,
            recommendation.ISBN,
            recommendation.ReadingCategory,
            recommendation.Authors,
            recommendation.PublishYear,
            recommendation.Page,
            recommendation.ResourceLink,
            recommendation.CoverImageUri
        );

        await Send.OkAsync(Result.Success(response), cancellation: ct);
    }
}
