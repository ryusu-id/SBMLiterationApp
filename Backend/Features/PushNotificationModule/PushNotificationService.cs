using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.TestModule;
using WebPush;

namespace PureTCOWebApp.Features.PushNotificationModule;

public class PushNotificationService(
    ApplicationDbContext dbContext,
    IOptions<VapidSettings> vapidOptions,
    ILogger<PushNotificationService> logger
) : IPushNotificationService
{
    public async Task SendAsync(string endpoint, string p256dh, string auth, PushPayload payload, CancellationToken ct = default)
    {
        var settings = vapidOptions.Value;
        if (!settings.IsConfigured)
        {
            logger.LogWarning("[Push] VAPID keys not configured — skipping send.");
            return;
        }

        var subscription = new PushSubscription(endpoint, p256dh, auth);
        var vapidDetails = new VapidDetails(settings.Subject, settings.PublicKey, settings.PrivateKey);
        var json = BuildPayloadJson(payload);

        try
        {
            var client = new WebPushClient();
            await client.SendNotificationAsync(subscription, json, vapidDetails, ct);
        }
        catch (WebPushException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Gone)
        {
            logger.LogWarning("[Push] Subscription expired (410 Gone), endpoint: {Endpoint}", endpoint[..Math.Min(endpoint.Length, 60)]);
            // Remove stale subscription from DB
            var stale = await dbContext.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint, ct);
            if (stale is not null)
            {
                dbContext.PushSubscriptions.Remove(stale);
                await dbContext.SaveChangesAsync(ct);
            }
        }
        catch (WebPushException ex)
        {
            logger.LogError(ex, "[Push] Failed to send to endpoint: {Endpoint} — {Status}", endpoint[..Math.Min(endpoint.Length, 60)], ex.StatusCode);
            throw;
        }
    }

    public async Task<(int Sent, int Failed)> BroadcastAsync(PushPayload payload, CancellationToken ct = default)
    {
        var settings = vapidOptions.Value;
        if (!settings.IsConfigured)
        {
            logger.LogWarning("[Push] VAPID keys not configured — skipping broadcast.");
            return (0, 0);
        }

        var subscriptions = await dbContext.PushSubscriptions.ToListAsync(ct);
        if (subscriptions.Count == 0)
        {
            logger.LogInformation("[Push] No subscriptions found for broadcast.");
            return (0, 0);
        }

        var vapidDetails = new VapidDetails(settings.Subject, settings.PublicKey, settings.PrivateKey);
        var json = BuildPayloadJson(payload);
        var staleEndpoints = new List<string>();
        int sent = 0, failed = 0;

        foreach (var sub in subscriptions)
        {
            try
            {
                var pushSub = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
                var client = new WebPushClient();
                await client.SendNotificationAsync(pushSub, json, vapidDetails, ct);
                sent++;
            }
            catch (WebPushException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Gone)
            {
                logger.LogWarning("[Push] Stale subscription removed: {Endpoint}", sub.Endpoint[..Math.Min(sub.Endpoint.Length, 60)]);
                staleEndpoints.Add(sub.Endpoint);
                failed++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[Push] Broadcast failed for endpoint: {Endpoint}", sub.Endpoint[..Math.Min(sub.Endpoint.Length, 60)]);
                failed++;
            }
        }

        // Clean up stale subscriptions
        if (staleEndpoints.Count > 0)
        {
            var stale = dbContext.PushSubscriptions.Where(s => staleEndpoints.Contains(s.Endpoint));
            dbContext.PushSubscriptions.RemoveRange(stale);
            await dbContext.SaveChangesAsync(ct);
        }

        logger.LogInformation("[Push] Broadcast complete — Sent: {Sent}, Failed: {Failed}", sent, failed);
        return (sent, failed);
    }

    public async Task SendToUserAsync(int userId, PushPayload payload, CancellationToken ct = default)
    {
        var subscriptions = await dbContext.PushSubscriptions
            .Where(s => s.UserId == userId)
            .ToListAsync(ct);

        foreach (var sub in subscriptions)
        {
            await SendAsync(sub.Endpoint, sub.P256dh, sub.Auth, payload, ct);
        }
    }

    private static string BuildPayloadJson(PushPayload payload) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            title = payload.Title,
            body = payload.Body,
            icon = payload.Icon,
            data = new { url = payload.Url },
            tag = "sigma-notification"
        });
}
