using FastEndpoints;
using FluentValidation;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.ReadingRecommendationModule.Endpoints;

public class UpdateReadingRecommendationRequestValidator : AbstractValidator<UpdateReadingRecommendationRequest>
{
    public UpdateReadingRecommendationRequestValidator()
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

public record UpdateReadingRecommendationRequest(
    int Id,
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

public record UpdateReadingRecommendationResponse(
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

public class UpdateReadingRecommendationEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<UpdateReadingRecommendationRequest, ApiResponse<UpdateReadingRecommendationResponse>>
{
    public override void Configure()
    {
        Put("{id}");
        Group<ReadingRecommendationEndpointGroup>();
        Roles("admin");
    }

    public override async Task HandleAsync(UpdateReadingRecommendationRequest req, CancellationToken ct)
    {
        var recommendation = await dbContext.ReadingRecommendations.FindAsync([req.Id], ct);

        if (recommendation is null)
        {
            var error = CrudDomainError.NotFound("ReadingRecommendation", req.Id);
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>((Result)error));
            return;
        }

        recommendation.Update(
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

        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(
            new UpdateReadingRecommendationResponse(
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
        ), cancellation: ct);
    }
}
