using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.PushNotificationModule.Endpoints;

public record GetSubscriberCountResponse(int Count);

/// <summary>
/// Returns the total number of active push subscriptions stored in DB.
/// Used by the admin push test page to show subscriber count.
///
/// Route: GET /api/push/subscriber-count
/// </summary>
public class GetSubscriberCountEndpoint(
    ApplicationDbContext dbContext
) : EndpointWithoutRequest<ApiResponse<GetSubscriberCountResponse>>
{
    public override void Configure()
    {
        Get("subscriber-count");
        Group<PushNotificationEndpointGroup>();
        Roles("admin");
        Summary(s => s.Summary = "Get total number of active push subscriptions.");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var count = await dbContext.PushSubscriptions.CountAsync(ct);
        await Send.OkAsync(Result.Success(new GetSubscriberCountResponse(count)), cancellation: ct);
    }
}
