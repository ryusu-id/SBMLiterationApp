using FastEndpoints;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Features.PushNotificationModule;

namespace PureTCOWebApp.Features.TestModule.Endpoints;

public record BroadcastPushNotificationRequest(
    string Title = "SIGMA Broadcast",
    string Body = "This is a broadcast push notification! 📢",
    string? Icon = "/favicon.svg",
    string? Url = "/"
);

public record BroadcastPushNotificationResponse(int Sent, int Failed, string Message);

/// <summary>
/// Admin test endpoint — sends a push notification to ALL stored subscriptions.
/// Uses the IPushNotificationService which reads from push_subscriptions table.
///
/// Route: POST /api/test-items/push-notification/broadcast
/// </summary>
public class BroadcastPushNotificationEndpoint(
    IPushNotificationService pushService,
    ILogger<BroadcastPushNotificationEndpoint> logger
) : Endpoint<BroadcastPushNotificationRequest, ApiResponse<BroadcastPushNotificationResponse>>
{
    public override void Configure()
    {
        Post("push-notification/broadcast");
        Group<TestModuleEndpointGroup>();
        Roles("admin");
        Summary(s =>
        {
            s.Summary = "[Test] Broadcast push notification to all subscribed users.";
            s.Description =
                "Sends a push notification to every subscription stored in push_subscriptions table. " +
                "Stale/expired subscriptions are automatically cleaned up. " +
                "Users must have subscribed via POST /api/push/subscribe first.";
        });
    }

    public override async Task HandleAsync(BroadcastPushNotificationRequest req, CancellationToken ct)
    {
        logger.LogInformation("[PushBroadcast] Starting broadcast: '{Title}'", req.Title);

        var payload = new PushPayload(req.Title, req.Body, req.Icon, req.Url);
        var (sent, failed) = await pushService.BroadcastAsync(payload, ct);

        if (sent == 0 && failed == 0)
        {
            await Send.OkAsync(
                Result.Failure<BroadcastPushNotificationResponse>(
                    new Error("Push.NoSubscribers", "No push subscriptions found. Users need to subscribe first by visiting the app.")),
                cancellation: ct);
            return;
        }

        logger.LogInformation("[PushBroadcast] Done — Sent: {Sent}, Failed: {Failed}", sent, failed);

        await Send.OkAsync(
            Result.Success(new BroadcastPushNotificationResponse(
                Sent: sent,
                Failed: failed,
                Message: $"Broadcast complete. Sent: {sent}, Failed/Expired: {failed}.")),
            cancellation: ct);
    }
}
