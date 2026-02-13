using FastEndpoints;
using FluentValidation;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.ReadingResourceModule.Endpoints.ReadingResourceEndpoints;

public class UpdateReadingResourceRequestValidator : AbstractValidator<UpdateReadingResourceRequest>
{
    public UpdateReadingResourceRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.ReadingCategory)
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

        RuleFor(x => x.CoverImageUri)
            .MaximumLength(500).WithMessage("Cover Image URI must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.CoverImageUri));
    }
}

public record UpdateReadingResourceRequest(
    int Id,
    string Title,
    string ReadingCategory,
    string Authors,
    string PublishYear,
    int Page,
    string CssClass,
    string? CoverImageUri
);

public record UpdateReadingResourceResponse(
    int Id,
    int UserId,
    string Title,
    string ReadingCategory,
    string Authors,
    string PublishYear,
    int Page,
    string CssClass,
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
        if (book is not null)
        {
            if (book.UserId != userId)
            {
                await Send.ForbiddenAsync(ct);
                return;
            }

            var valCtx = FastEndpoints.ValidationContext<UpdateReadingResourceRequest>.Instance;
            if (string.IsNullOrEmpty(req.ReadingCategory))
            {
                valCtx.AddError(e => e.ReadingCategory, "Reading Category is required for Book.");
            }
            valCtx.ThrowIfAnyErrors();
            
            book.Update(req.Title, req.ReadingCategory, req.Authors, req.PublishYear, req.Page, req.CssClass, req.CoverImageUri);
            
            var result = await unitOfWork.SaveChangesAsync(ct);
            if (result.IsFailure)
            {
                await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
                return;
            }

            var response = new UpdateReadingResourceResponse(
                book.Id, book.UserId, book.Title, book.ReadingCategory,
                book.Authors, book.PublishYear, book.Page, book.CssClass, book.CoverImageUri, "BOOK"
            );
            await Send.OkAsync(Result.Success(response), cancellation: ct);
            return;
        }

        // Try to find as JournalPaper
        var journal = await dbContext.JournalPapers.FindAsync([req.Id], ct);
        if (journal is not null)
        {
            if (journal.UserId != userId)
            {
                await Send.ForbiddenAsync(ct);
                return;
            }
            
            journal.Update(req.Title, req.ReadingCategory, req.Authors, req.PublishYear, req.Page, req.CssClass, req.CoverImageUri);
            
            var result = await unitOfWork.SaveChangesAsync(ct);
            if (result.IsFailure)
            {
                await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
                return;
            }

            var response = new UpdateReadingResourceResponse(
                journal.Id, journal.UserId, journal.Title, journal.ReadingCategory,
                journal.Authors, journal.PublishYear, journal.Page, journal.CssClass, journal.CoverImageUri, "JOURNAL"
            );
            await Send.OkAsync(Result.Success(response), cancellation: ct);
            return;
        }

        // Not found
        var error = CrudDomainError.NotFound("ReadingResource", req.Id);
        await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>((Result)error));
    }
}