using FastEndpoints;
using FluentValidation;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain;

namespace PureTCOWebApp.Features.ReadingResourceModule.Endpoints;

public class UpdateReadingResourceRequestValidator : AbstractValidator<UpdateReadingResourceRequest>
{
    public UpdateReadingResourceRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.ISBN)
            .NotEmpty().WithMessage("ISBN is required.")
            .MaximumLength(50).WithMessage("ISBN must not exceed 50 characters.");

        RuleFor(x => x.BookCategory)
            .NotEmpty().WithMessage("Book Category is required.")
            .MaximumLength(100).WithMessage("Book Category must not exceed 100 characters.");

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

public record UpdateReadingResourceRequest(
    int Id,
    string Title,
    string ISBN,
    string BookCategory,
    string Authors,
    string PublishYear,
    int Page,
    string? ResourceLink,
    string? CoverImageUri
);

public record UpdateReadingResourceResponse(
    int Id,
    int UserId,
    string Title,
    string ISBN,
    string BookCategory,
    string Authors,
    string PublishYear,
    int Page,
    string? ResourceLink,
    string? CoverImageUri,
    string ResourceType
);

public class UpdateReadingResourceEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<UpdateReadingResourceRequest, ApiResponse<UpdateReadingResourceResponse>>
{
    public override void Configure()
    {
        Put("{id}");
        Group<ReadingResourceEndpointGroup>();
    }

    public override async Task HandleAsync(UpdateReadingResourceRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        
        // Try to find as Book first
        var book = await dbContext.Books.FindAsync([req.Id], ct);
        if (book != null)
        {
            if (book.UserId != userId)
            {
                await Send.ForbiddenAsync(ct);
                return;
            }
            
            book.Update(req.Title, req.ISBN, req.BookCategory, req.Authors, req.PublishYear, req.Page, req.ResourceLink, req.CoverImageUri);
            
            var result = await unitOfWork.SaveChangesAsync(ct);
            if (result.IsFailure)
            {
                await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
                return;
            }

            var response = new UpdateReadingResourceResponse(
                book.Id, book.UserId, book.Title, book.ISBN, book.BookCategory,
                book.Authors, book.PublishYear, book.Page, book.ResourceLink, book.CoverImageUri, "BOOK"
            );
            await Send.OkAsync(Result.Success(response), cancellation: ct);
            return;
        }

        // Try to find as JournalPaper
        var journal = await dbContext.JournalPapers.FindAsync([req.Id], ct);
        if (journal != null)
        {
            if (journal.UserId != userId)
            {
                await Send.ForbiddenAsync(ct);
                return;
            }
            
            journal.Update(req.Title, req.ISBN, req.BookCategory, req.Authors, req.PublishYear, req.Page, req.ResourceLink, req.CoverImageUri);
            
            var result = await unitOfWork.SaveChangesAsync(ct);
            if (result.IsFailure)
            {
                await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
                return;
            }

            var response = new UpdateReadingResourceResponse(
                journal.Id, journal.UserId, journal.Title, journal.ISBN, journal.BookCategory,
                journal.Authors, journal.PublishYear, journal.Page, journal.ResourceLink, journal.CoverImageUri, "JOURNAL"
            );
            await Send.OkAsync(Result.Success(response), cancellation: ct);
            return;
        }

        // Not found
        var error = CrudDomainError.NotFound("ReadingResource", req.Id);
        await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>((Result)error));
    }
}