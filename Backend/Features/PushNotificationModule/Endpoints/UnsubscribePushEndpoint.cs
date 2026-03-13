using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.PushNotificationModule.Endpoints;

public record UnsubscribePushRequest(string Endpoint);

/// <summary>
/// Removes a browser push subscription from the database.
/// Called when user revokes notification permission or logs out.
///
/// Route: DELETE /api/push/unsubscribe
/// </summary>
public class UnsubscribePushEndpoint(
    ApplicationDbContext dbContext,
    ILogger<UnsubscribePushEndpoint> logger
) : Endpoint<UnsubscribePushRequest, ApiResponse>
{
    public override void Configure()
    {
        Delete("unsubscribe");
        Group<PushNotificationEndpointGroup>();
        Summary(s => s.Summary = "Remove a push subscription for the current user.");
    }

    public override async Task HandleAsync(UnsubscribePushRequest req, CancellationToken ct)
    {
        var sub = await dbContext.PushSubscriptions
            .FirstOrDefaultAsync(s => s.Endpoint == req.Endpoint, ct);

        if (sub is not null)
        {
            dbContext.PushSubscriptions.Remove(sub);
            await dbContext.SaveChangesAsync(ct);
            logger.LogInformation("[Push] Subscription removed: {Endpoint}", req.Endpoint[..Math.Min(req.Endpoint.Length, 60)]);
        }

        await Send.OkAsync(Result.Success("Unsubscribed."), cancellation: ct);
    }
}
