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
    string Insight);

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
            .MaximumLength(1000)
            .WithMessage("Insight must not exceed 1000 characters.");
    }
}

public class CreateReadingReportEndpoint(ApplicationDbContext context)
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
        
        var resourceExists = await context.Set<ReadingResourceBase>()
            .AnyAsync(r => r.Id == req.ReadingResourceId && r.UserId == userId, ct);

        if (!resourceExists)
        {
            var error = CrudDomainError.NotFound("ReadingResource", req.ReadingResourceId);
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>((Result)error));
            return;
        }

        var report = ReadingReport.Create(
            userId,
            req.ReadingResourceId,
            req.CurrentPage,
            req.Insight);

        context.ReadingReports.Add(report);
        await context.SaveChangesAsync(ct);

        await Send.OkAsync(Result.Success(), ct);
    }
}