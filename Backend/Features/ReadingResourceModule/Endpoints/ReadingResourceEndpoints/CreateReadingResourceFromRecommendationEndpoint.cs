using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain;

namespace PureTCOWebApp.Features.ReadingResourceModule.Endpoints.ReadingResourceEndpoints;

public class CreateReadingResourceFromRecommendationRequestValidator : AbstractValidator<CreateReadingResourceFromRecommendationRequest>
{
    public CreateReadingResourceFromRecommendationRequestValidator()
    {
        RuleFor(x => x.RecommendationId)
            .GreaterThan(0).WithMessage("Recommendation ID must be greater than 0.");

        RuleFor(x => x.CssClass)
            .MaximumLength(100).WithMessage("CSS Class must not exceed 100 characters.");
    }
}

public record CreateReadingResourceFromRecommendationRequest(
    int RecommendationId,
    string? CssClass
);

public record CreateReadingResourceFromRecommendationResponse(
    int Id,
    int UserId,
    string Title,
    string ISBN,
    string? ReadingCategory,
    string Authors,
    string PublishYear,
    int Page,
    string CssClass,
    string? ResourceLink,
    string? CoverImageUri
);

public class CreateReadingResourceFromRecommendationEndpoint(
    ApplicationDbContext _dbContext,
    UnitOfWork _unitOfWork
) : Endpoint<CreateReadingResourceFromRecommendationRequest, ApiResponse<CreateReadingResourceFromRecommendationResponse>>
{
    public override void Configure()
    {
        Post("from-recommendation");
        Group<ReadingResourceEndpointGroup>();
    }

    public override async Task HandleAsync(CreateReadingResourceFromRecommendationRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);

        var recommendation = await _dbContext.ReadingRecommendations
            .FirstOrDefaultAsync(r => r.Id == req.RecommendationId, ct);

        if (recommendation is null)
        {
            await Send.ResultAsync(TypedResults.NotFound<ApiResponse>(
                Result.Failure(new Error("NotFound", "Reading recommendation not found"))));
            return;
        }

        if (
            await _dbContext.Books.AnyAsync(b => b.UserId == userId && b.ISBN == recommendation.ISBN, ct)
        )
        {
            await Send.ResultAsync(TypedResults.Conflict<ApiResponse>(
                Result.Failure(new Error("Conflict", "You have already added this reading resource"))));
            return;
        }

        var book = Book.Create(
            userId,
            recommendation.Title,
            recommendation.ISBN,
            recommendation.ReadingCategory,
            recommendation.Authors,
            recommendation.PublishYear,
            recommendation.Page,
            req.CssClass ?? "",
            recommendation.ResourceLink,
            recommendation.CoverImageUri
        );

        await _dbContext.AddAsync(book, ct);
        var result = await _unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(
            new CreateReadingResourceFromRecommendationResponse(
                book.Id,
                book.UserId,
                book.Title,
                book.ISBN,
                book.ReadingCategory,
                book.Authors,
                book.PublishYear,
                book.Page,
                book.CssClass,
                book.ResourceLink,
                book.CoverImageUri
            )
        ), cancellation: ct);
    }
}