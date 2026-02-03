using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingRecommendationModule.Domain;

namespace PureTCOWebApp.Features.ReadingRecommendationModule.Endpoints;

public class CreateReadingRecommendationRequestValidator : AbstractValidator<CreateReadingRecommendationRequest>
{
    public CreateReadingRecommendationRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.ISBN)
            .NotEmpty().WithMessage("ISBN is required.")
            .MaximumLength(50).WithMessage("ISBN must not exceed 50 characters.");

        RuleFor(x => x.ReadingCategory)
            .NotEmpty().WithMessage("Reading Category is required.")
            .MaximumLength(100).WithMessage("Reading Category must not exceed 100 characters.");

        RuleFor(x => x.Authors)
            .NotEmpty().WithMessage("Authors is required.")
            .MaximumLength(300).WithMessage("Authors must not exceed 300 characters.");

        RuleFor(x => x.PublishYear)
            .NotEmpty().WithMessage("Publish Year is required.")
            .MaximumLength(10).WithMessage("Publish Year must not exceed 10 characters.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.ResourceLink)
            .MaximumLength(500).WithMessage("Resource Link must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.ResourceLink));

        RuleFor(x => x.CoverImageUri)
            .MaximumLength(500).WithMessage("Cover Image URI must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.CoverImageUri));
    }
}

public record CreateReadingRecommendationRequest(
    string Title,
    string ISBN,
    string ReadingCategory,
    string Authors,
    string PublishYear,
    int Page,
    string? ResourceLink = null,
    string? CoverImageUri = null,
    int Exp = 0
);

public record CreateReadingRecommendationResponse(
    int Id,
    string Title,
    string ISBN,
    string ReadingCategory,
    string Authors,
    string PublishYear,
    int Page,
    string? ResourceLink,
    string? CoverImageUri,
    int Exp
);

public class CreateReadingRecommendationEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<CreateReadingRecommendationRequest, ApiResponse<CreateReadingRecommendationResponse>>
{
    public override void Configure()
    {
        Post("");
        Group<ReadingRecommendationEndpointGroup>();
        Roles("admin");
    }

    public override async Task HandleAsync(CreateReadingRecommendationRequest req, CancellationToken ct)
    {
        if (await dbContext.ReadingRecommendations.AnyAsync(x => x.ISBN == req.ISBN, ct))
        {
            await Send.ResultAsync(TypedResults.Conflict<ApiResponse>((Result)CrudDomainError.Duplicate("ReadingRecommendation", "ISBN")));
            return;
        }

        var recommendation = ReadingRecommendation.Create(
            req.Title,
            req.ISBN,
            req.ReadingCategory,
            req.Authors,
            req.PublishYear,
            req.Page,
            req.ResourceLink,
            req.CoverImageUri,
            req.Exp
        );

        await dbContext.AddAsync(recommendation, ct);
        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(
            new CreateReadingRecommendationResponse(
                recommendation.Id,
                recommendation.Title,
                recommendation.ISBN,
                recommendation.ReadingCategory,
                recommendation.Authors,
                recommendation.PublishYear,
                recommendation.Page,
                recommendation.ResourceLink,
                recommendation.CoverImageUri,
                recommendation.Exp
            )
        ), ct);
    }
}
