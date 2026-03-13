using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.PushNotificationModule.Domain;

namespace PureTCOWebApp.Features.PushNotificationModule.Endpoints;

public record SubscribePushRequest(string Endpoint, string P256dh, string Auth, string? UserAgent = null);
public record SubscribePushResponse(bool IsNew, string Message);

/// <summary>
/// Saves a browser push subscription to the database for the current user.
/// Called automatically by the frontend after login.
/// Idempotent — calling multiple times with the same endpoint is safe.
///
/// Route: POST /api/push/subscribe
/// </summary>
public class SubscribePushEndpoint(
    ApplicationDbContext dbContext,
    ILogger<SubscribePushEndpoint> logger
) : Endpoint<SubscribePushRequest, ApiResponse<SubscribePushResponse>>
{
    public override void Configure()
    {
        Post("subscribe");
        Group<PushNotificationEndpointGroup>();
        Summary(s =>
        {
            s.Summary = "Save a browser push subscription for the current user.";
            s.Description = "Idempotent — re-calling with the same endpoint updates the keys.";
        });
    }

    public override async Task HandleAsync(SubscribePushRequest req, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var existing = await dbContext.PushSubscriptions
            .FirstOrDefaultAsync(s => s.Endpoint == req.Endpoint, ct);

        bool isNew;
        if (existing is null)
        {
            dbContext.PushSubscriptions.Add(new PushSubscription
            {
                UserId = userId,
                Endpoint = req.Endpoint,
                P256dh = req.P256dh,
                Auth = req.Auth,
                UserAgent = req.UserAgent,
                CreatedAt = DateTime.UtcNow
            });
            isNew = true;
            logger.LogInformation("[Push] New subscription registered for UserId: {UserId}", userId);
        }
        else
        {
            // Update keys in case they rotated
            existing.P256dh = req.P256dh;
            existing.Auth = req.Auth;
            existing.UserId = userId;
            existing.UserAgent = req.UserAgent;
            isNew = false;
            logger.LogInformation("[Push] Subscription updated for UserId: {UserId}", userId);
        }

        await dbContext.SaveChangesAsync(ct);

        await Send.OkAsync(
            Result.Success(new SubscribePushResponse(isNew, isNew ? "Subscription saved." : "Subscription updated.")),
            cancellation: ct);
    }
}
