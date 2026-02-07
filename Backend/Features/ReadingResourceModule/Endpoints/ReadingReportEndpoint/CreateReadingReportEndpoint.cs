using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain;
using PureTCOWebApp.Features.ReadingResourceModule.Domain.Entities;

namespace PureTCOWebApp.Features.ReadingResourceModule.Endpoints.ReadingReportEndpoint;

public record CreateReadingReportRequest(
    int ReadingResourceId,
    int CurrentPage,
    string Insight,
    int TimeSpent);

public class CreateReadingReportValidator : AbstractValidator<CreateReadingReportRequest>
{
    public CreateReadingReportValidator()
    {
        RuleFor(x => x.ReadingResourceId)
            .GreaterThan(0)
            .WithMessage("Reading Resource ID must be greater than 0.");

        RuleFor(x => x.CurrentPage)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Current page must be 0 or greater.");

        RuleFor(x => x.Insight)
            .NotEmpty()
            .WithMessage("Insight is required.")
            .MaximumLength(5000)
            .WithMessage("Insight must not exceed 5000 characters.");

        RuleFor(x => x.TimeSpent)
            .GreaterThan(0)
            .WithMessage("Time spent must be greater than 0 minutes.");
    }
}

public class CreateReadingReportEndpoint(ApplicationDbContext context, UnitOfWork unitOfWork)
    : Endpoint<CreateReadingReportRequest, ApiResponse>
{
    public override void Configure()
    {
        Post("/reports");
        Group<ReadingResourceEndpointGroup>();
        Validator<CreateReadingReportValidator>();
    }

    public override async Task HandleAsync(CreateReadingReportRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        
        var resource = await context.Set<ReadingResourceBase>()
            .FirstOrDefaultAsync(r => r.Id == req.ReadingResourceId && r.UserId == userId, ct);

        if (resource == null)
        {
            var error = CrudDomainError.NotFound("ReadingResource", req.ReadingResourceId);
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>((Result)error));
            return;
        }

        // Validate: CurrentPage must be >= latest CurrentPage
        var latestReport = await context.ReadingReports
            .Where(r => r.UserId == userId && r.ReadingResourceId == req.ReadingResourceId)
            .OrderByDescending(r => r.CreateTime)
            .FirstOrDefaultAsync(ct);

        if (latestReport != null && req.CurrentPage < latestReport.CurrentPage)
        {
            var error = new Error("InvalidCurrentPage", 
                $"Current page ({req.CurrentPage}) must be greater than or equal to the latest page ({latestReport.CurrentPage}).");
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(Result.Failure(error)));
            return;
        }

        // Validate: CurrentPage must not exceed total pages
        if (req.CurrentPage > resource.Page)
        {
            var error = new Error("CurrentPageExceedsTotalPages", 
                $"Current page ({req.CurrentPage}) cannot exceed total pages ({resource.Page}).");
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(Result.Failure(error)));
            return;
        }

        var report = ReadingReport.Create(
            userId,
            req.ReadingResourceId,
            req.CurrentPage,
            req.Insight,
            req.TimeSpent,
            latestReport?.CurrentPage ?? 0
        );

        context.ReadingReports.Add(report);
        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(), cancellation: ct);
    }
}