using FastEndpoints;
using Microsoft.Extensions.Options;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using WebPush;

namespace PureTCOWebApp.Features.TestModule.Endpoints;

public record PushSubscriptionDto(string Endpoint, string P256dh, string Auth);

public record SendTestPushNotificationRequest(
    PushSubscriptionDto Subscription,
    string Title = "SIGMA Test Push",
    string Body = "Push notification is working! 🎉",
    string? Icon = "/favicon.svg",
    string? Url = "/"
);

public record SendTestPushNotificationResponse(bool Success, string Message);

/// <summary>
/// Test endpoint for verifying Web Push (VAPID) delivery end-to-end.
///
/// How to use:
/// 1. Call GET /api/test-items/push-notification/vapid-key to get the VAPID public key.
/// 2. In the browser DevTools console, subscribe:
///    const reg = await navigator.serviceWorker.ready;
///    const sub = await reg.pushManager.subscribe({ userVisibleOnly: true, applicationServerKey: '&lt;PUBLIC_KEY&gt;' });
///    console.log(JSON.stringify(sub));
/// 3. POST the subscription JSON + title/body here — the browser will receive the notification.
///
/// Route: POST /api/test-items/push-notification
/// </summary>
public class SendTestPushNotificationEndpoint(
    IOptions<VapidSettings> vapidOptions,
    ILogger<SendTestPushNotificationEndpoint> logger
) : Endpoint<SendTestPushNotificationRequest, ApiResponse<SendTestPushNotificationResponse>>
{
    public override void Configure()
    {
        Post("push-notification");
        Group<TestModuleEndpointGroup>();
        Roles("admin");
        Summary(s =>
        {
            s.Summary = "[Test] Send a Web Push notification to a given browser subscription.";
            s.Description =
                "Provide a browser PushSubscription object (endpoint, p256dh, auth) and a message. " +
                "The backend will deliver a push notification via VAPID. " +
                "Requires VAPID keys to be configured in appsettings (Vapid section). " +
                "Get the public key from GET /api/test-items/push-notification/vapid-key first.";
        });
    }

    public override async Task HandleAsync(SendTestPushNotificationRequest req, CancellationToken ct)
    {
        var settings = vapidOptions.Value;

        if (!settings.IsConfigured)
        {
            logger.LogWarning("[PushNotificationTest] VAPID keys are not configured in appsettings.json.");
            await Send.OkAsync(
                Result.Failure<SendTestPushNotificationResponse>(new Error(
                    "Vapid.NotConfigured",
                    "VAPID keys are not configured. " +
                    "Add Subject, PublicKey, and PrivateKey under the 'Vapid' section in appsettings.json. " +
                    "Generate keys with VapidHelper.GenerateVapidKeys() from the WebPush library.")),
                cancellation: ct);
            return;
        }

        var subscription = new PushSubscription(
            req.Subscription.Endpoint,
            req.Subscription.P256dh,
            req.Subscription.Auth
        );

        var vapidDetails = new VapidDetails(
            settings.Subject,
            settings.PublicKey,
            settings.PrivateKey
        );

        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            title = req.Title,
            body = req.Body,
            icon = req.Icon,
            data = new { url = req.Url },
            tag = "sigma-test-push"
        });

        try
        {
            var client = new WebPushClient();
            await client.SendNotificationAsync(subscription, payload, vapidDetails, ct);

            logger.LogInformation(
                "[PushNotificationTest] Push notification sent successfully to endpoint: {Endpoint}",
                req.Subscription.Endpoint[..Math.Min(req.Subscription.Endpoint.Length, 60)]);

            await Send.OkAsync(
                Result.Success(new SendTestPushNotificationResponse(true, "Push notification delivered successfully.")),
                cancellation: ct);
        }
        catch (WebPushException ex)
        {
            logger.LogError(ex,
                "[PushNotificationTest] WebPush delivery failed. StatusCode: {StatusCode}",
                ex.StatusCode);

            var detail = ex.StatusCode switch
            {
                System.Net.HttpStatusCode.Gone => "Subscription has expired or is no longer valid (410 Gone). Re-subscribe in the browser.",
                System.Net.HttpStatusCode.Unauthorized => "VAPID authentication failed. Check your PublicKey and PrivateKey in appsettings.json.",
                System.Net.HttpStatusCode.RequestEntityTooLarge => "Payload is too large. Reduce the message size.",
                _ => $"Push service returned {(int)ex.StatusCode}: {ex.Message}"
            };

            await Send.OkAsync(
                Result.Failure<SendTestPushNotificationResponse>(new Error("Push.DeliveryFailed", detail)),
                cancellation: ct);
        }
    }
}