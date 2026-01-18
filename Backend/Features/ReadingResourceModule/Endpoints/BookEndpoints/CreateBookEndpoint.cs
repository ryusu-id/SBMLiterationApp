using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain;

namespace PureTCOWebApp.Features.ReadingResourceModule.Endpoints;

public class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookRequestValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId is required.");

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
    }
}

public record CreateBookRequest(
    int UserId,
    string Title,
    string ISBN,
    string BookCategory,
    string Authors,
    string PublishYear,
    int Page,
    string? ResourceLink
);

public record CreateBookResponse(
    int Id,
    int UserId,
    string Title,
    string ISBN,
    string BookCategory,
    string Authors,
    string PublishYear,
    int Page,
    string? ResourceLink
);

public class CreateBookEndpoint(
    ApplicationDbContext _dbContext,
    UnitOfWork _unitOfWork
) : Endpoint<CreateBookRequest, ApiResponse<CreateBookResponse>>
{
    public override void Configure()
    {
        Post("books");
        Group<ReadingResourceEndpointGroup>();
    }

    public override async Task HandleAsync(CreateBookRequest req, CancellationToken ct)
    {
        if (await _dbContext.Books.AnyAsync(x => x.ISBN == req.ISBN, ct))
        {
            await Send.ResultAsync(TypedResults.Conflict<ApiResponse>((Result)CrudDomainError.Duplicate("Book", "ISBN")));
            return;
        }

        var book = Book.Create(
            req.UserId,
            req.Title,
            req.ISBN,
            req.BookCategory,
            req.Authors,
            req.PublishYear,
            req.Page,
            req.ResourceLink
        );

        await _dbContext.AddAsync(book, ct);
        var result = await _unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(
            new CreateBookResponse(
                book.Id,
                book.UserId,
                book.Title,
                book.ISBN,
                book.BookCategory,
                book.Authors,
                book.PublishYear,
                book.Page,
                book.ResourceLink
            )
        ), cancellation: ct);
    }
}