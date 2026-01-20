using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.ReadingResourceModule.Endpoints.ReadingReportEndpoint;

public record UpdateReadingReportRequest(
    int CurrentPage,
    string Insight);

public class UpdateReadingReportValidator : AbstractValidator<UpdateReadingReportRequest>
{
    public UpdateReadingReportValidator()
    {
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

public class UpdateReadingReportEndpoint(ApplicationDbContext context)
    : Endpoint<UpdateReadingReportRequest, ApiResponse>
{
    public override void Configure()
    {
        Put("/reports/{id}");
        Group<ReadingResourceEndpointGroup>();
        Validator<UpdateReadingReportValidator>();
    }

    public override async Task HandleAsync(UpdateReadingReportRequest req, CancellationToken ct)
    {
        var id = Route<int>("id");
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        
        var report = await context.ReadingReports
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (report is null)
        {
            var error = CrudDomainError.NotFound("ReadingReport", id);
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>((Result)error));
            return;
        }
        
        if (report.UserId != userId)
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        report.Update(req.CurrentPage, req.Insight);
        await context.SaveChangesAsync(ct);

        await Send.OkAsync(Result.Success(), ct);
    }
}