using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain;

namespace PureTCOWebApp.Features.ReadingResourceModule.Endpoints.ReadingResourceEndpoints;

public record ReadingResourceWithProgress(
    int Id,
    int UserId,
    string Title,
    string ISBN,
    string ReadingCategory,
    string Authors,
    string PublishYear,
    int Page,
    string? ResourceLink,
    string? CoverImageUri,
    string CssClass,
    int? LastReadPage
);

public class GetReadingResourceByIdEndpoint(ApplicationDbContext context)
    : EndpointWithoutRequest<ApiResponse<ReadingResourceWithProgress>>
{
    public override void Configure()
    {
        Get("/{id}");
        Group<ReadingResourceEndpointGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        
        var resource = await context.Set<ReadingResourceBase>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);

        if (resource is null)
        {
            var error = CrudDomainError.NotFound("ReadingResource", id);
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>((Result)error));
            return;
        }

        var lastReadPage = await context.ReadingReports
            .Where(r => r.ReadingResourceId == id && r.UserId == userId)
            .OrderByDescending(r => r.ReportDate)
            .Select(r => (int?)r.CurrentPage)
            .FirstOrDefaultAsync(ct);

        var response = new ReadingResourceWithProgress(
            resource.Id,
            resource.UserId,
            resource.Title,
            resource.ISBN,
            resource.ReadingCategory,
            resource.Authors,
            resource.PublishYear,
            resource.Page,
            resource.ResourceLink,
            resource.CoverImageUri,
            resource.CssClass,
            lastReadPage
        );

        await Send.OkAsync(Result.Success(response), cancellation: ct);
    }
}