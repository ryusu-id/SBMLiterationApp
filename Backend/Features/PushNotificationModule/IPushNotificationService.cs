namespace PureTCOWebApp.Features.PushNotificationModule;

public record PushPayload(
    string Title,
    string Body,
    string? Icon = "/favicon.svg",
    string? Url = "/"
);

public interface IPushNotificationService
{
    /// <summary>Send a push notification to a single subscription endpoint.</summary>
    Task SendAsync(string endpoint, string p256dh, string auth, PushPayload payload, CancellationToken ct = default);

    /// <summary>Send a push notification to all stored subscriptions. Returns count of successful sends.</summary>
    Task<(int Sent, int Failed)> BroadcastAsync(PushPayload payload, CancellationToken ct = default);

    /// <summary>Send push to all subscriptions belonging to a specific user.</summary>
    Task SendToUserAsync(int userId, PushPayload payload, CancellationToken ct = default);
}
