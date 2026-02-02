using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.DailyReadsModule.Domain;

namespace PureTCOWebApp.Features.DailyReadsModule.Endpoints.DailyReadEndpoints;

public class CreateDailyReadRequestValidator : AbstractValidator<CreateDailyReadRequest>
{
    public CreateDailyReadRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.");

        RuleFor(x => x.CoverImg)
            .MaximumLength(500).WithMessage("CoverImg must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.CoverImg));

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Category));

        RuleFor(x => x.Exp)
            .GreaterThanOrEqualTo(0).WithMessage("Exp must be greater than or equal to 0.");

        RuleFor(x => x.MinimalCorrectAnswer)
            .GreaterThanOrEqualTo(0).WithMessage("MinimalCorrectAnswer must be greater than or equal to 0.");
    }
}

public record CreateDailyReadRequest(
    string Title,
    string Content,
    DateOnly Date,
    string? CoverImg = null,
    string? Category = null,
    decimal Exp = 0,
    int MinimalCorrectAnswer = 0
);

public record CreateDailyReadResponse(int Id, string Title, DateOnly Date);

public class CreateDailyReadEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<CreateDailyReadRequest, ApiResponse<CreateDailyReadResponse>>
{
    public override void Configure()
    {
        Post("");
        Group<DailyReadsEndpointGroup>();
        Roles("admin");
    }

    public override async Task HandleAsync(CreateDailyReadRequest req, CancellationToken ct)
    {
        if (await dbContext.DailyReads.AnyAsync(x => x.Date == req.Date, ct))
        {
            await Send.ResultAsync(TypedResults.Conflict<ApiResponse>((Result)CrudDomainError.Duplicate("DailyRead", "Date")));
            return;
        }

        var dailyRead = DailyRead.Create(req.Title, req.Content, req.Date, req.CoverImg, req.Category, req.Exp, req.MinimalCorrectAnswer);

        await dbContext.AddAsync(dailyRead, ct);
        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(
            new CreateDailyReadResponse(dailyRead.Id, dailyRead.Title, dailyRead.Date)
        ), cancellation: ct);
    }
}
