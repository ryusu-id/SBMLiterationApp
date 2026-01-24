using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain;

namespace PureTCOWebApp.Features.ReadingResourceModule.Endpoints.JournalPaperEndpoints;

public class CreateJournalPaperRequestValidator : AbstractValidator<CreateJournalPaperRequest>
{
    public CreateJournalPaperRequestValidator()
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

        RuleFor(x => x.CssClass)
            .NotEmpty().WithMessage("CSS Class is required.")
            .MaximumLength(100).WithMessage("CSS Class must not exceed 100 characters.");

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
            .When(x => !string.IsNullOrEmpty(x.ResourceLink));

        RuleFor(x => x.CoverImageUri)
            .MaximumLength(500).WithMessage("Cover Image URI must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.CoverImageUri));
    }
}

public record CreateJournalPaperRequest(
    string Title,
    string ISBN,
    string ReadingCategory,
    string Authors,
    string PublishYear,
    int Page,
    string CssClass,
    string? ResourceLink,
    string? CoverImageUri
);

public record CreateJournalPaperResponse(
    int Id,
    int UserId,
    string Title,
    string ISBN,
    string ReadingCategory,
    string Authors,
    string PublishYear,
    int Page,
    string CssClass,
    string? ResourceLink,
    string? CoverImageUri
);

public class CreateJournalPaperEndpoint(
    ApplicationDbContext _dbContext,
    UnitOfWork _unitOfWork
) : Endpoint<CreateJournalPaperRequest, ApiResponse<CreateJournalPaperResponse>>
{
    public override void Configure()
    {
        Post("journals");
        Group<ReadingResourceEndpointGroup>();
    }

    public override async Task HandleAsync(CreateJournalPaperRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        
        if (await _dbContext.JournalPapers.AnyAsync(x => x.ISBN == req.ISBN, ct))
        {
            await Send.ResultAsync(TypedResults.Conflict<ApiResponse>((Result)CrudDomainError.Duplicate("JournalPaper", "ISBN")));
            return;
        }

        var journalPaper = JournalPaper.Create(
            userId,
            req.Title,
            req.ISBN,
            req.ReadingCategory,
            req.Authors,
            req.PublishYear,
            req.Page,
            req.CssClass,
            req.ResourceLink,
            req.CoverImageUri
        );

        await _dbContext.AddAsync(journalPaper, ct);
        var result = await _unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(
            new CreateJournalPaperResponse(
                journalPaper.Id,
                journalPaper.UserId,
                journalPaper.Title,
                journalPaper.ISBN,
                journalPaper.ReadingCategory,
                journalPaper.Authors,
                journalPaper.PublishYear,
                journalPaper.Page,
                journalPaper.CssClass,
                journalPaper.ResourceLink,
                journalPaper.CoverImageUri
            )
        ), cancellation: ct);
    }
}