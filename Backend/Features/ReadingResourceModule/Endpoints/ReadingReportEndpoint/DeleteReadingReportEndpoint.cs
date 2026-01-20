using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain.Entities;

namespace PureTCOWebApp.Features.ReadingResourceModule.Endpoints.ReadingReportEndpoint;

public class DeleteReadingReportEndpoint(ApplicationDbContext context)
    : EndpointWithoutRequest<ApiResponse>
{
    public override void Configure()
    {
        Delete("/reports/{id}");
        Group<ReadingResourceEndpointGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        
        var report = await context.ReadingReports
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (report is null)
        {
            var error = CrudDomainError.NotFound(nameof(ReadingReport), id);
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>((Result)error));
            return;
        }
        
        if (report.UserId != userId)
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        context.Remove(report);
        await context.SaveChangesAsync(ct);

        await Send.OkAsync(Result.Success(), ct);
    }
}