using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain;

namespace PureTCOWebApp.Features.ReadingResourceModule.Endpoints.ReadingResourceEndpoints;

public class GetReadingResourceByIdEndpoint(ApplicationDbContext context)
    : EndpointWithoutRequest<ReadingResourceBase>
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

        await Send.OkAsync(resource, ct);
    }
}