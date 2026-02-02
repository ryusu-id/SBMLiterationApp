using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.DailyReadsModule.Endpoints.DailyReadEndpoints;

public class UpdateDailyReadRequestValidator : AbstractValidator<UpdateDailyReadRequest>
{
    public UpdateDailyReadRequestValidator()
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

public record UpdateDailyReadRequest(
    string Title,
    string Content,
    DateOnly Date,
    string? CoverImg = null,
    string? Category = null,
    decimal Exp = 0,
    int MinimalCorrectAnswer = 0
);

public class UpdateDailyReadEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<UpdateDailyReadRequest, ApiResponse>
{
    public override void Configure()
    {
        Put("{id}");
        Group<DailyReadsEndpointGroup>();
        Roles("admin");
    }

    public override async Task HandleAsync(UpdateDailyReadRequest req, CancellationToken ct)
    {
        var id = Route<int>("id");
        var dailyRead = await dbContext.DailyReads.FindAsync([id], ct);

        if (dailyRead == null)
        {
            await Send.ResultAsync(TypedResults.NotFound<ApiResponse>((Result)CrudDomainError.NotFound("DailyRead", id)));
            return;
        }

        if (await dbContext.DailyReads.AnyAsync(x => x.Date == req.Date && x.Id != id, ct))
        {
            await Send.ResultAsync(TypedResults.Conflict<ApiResponse>((Result)CrudDomainError.Duplicate("DailyRead", "Date")));
            return;
        }

        dailyRead.Update(req.Title, req.Content, req.Date, req.CoverImg, req.Category, req.Exp, req.MinimalCorrectAnswer);

        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(), cancellation: ct);
    }
}
