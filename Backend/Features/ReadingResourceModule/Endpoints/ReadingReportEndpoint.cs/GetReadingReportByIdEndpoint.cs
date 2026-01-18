using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain.Entities;

namespace PureTCOWebApp.Features.ReadingResourceModule.Endpoints.ReadingReportEndpoint.cs;

public class GetReadingReportByIdEndpoint(ApplicationDbContext context)
    : EndpointWithoutRequest<ReadingReport>
{
    public override void Configure()
    {
        Get("/reports/{id}");
        Group<ReadingResourceEndpointGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        
        var report = await context.ReadingReports
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (report is null)
        {
            var error = CrudDomainError.NotFound("ReadingReport", id);
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>((Result)error));
            return;
        }

        await Send.OkAsync(report, ct);
    }
}