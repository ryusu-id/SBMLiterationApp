using PureTCOWebApp.Features.Auth.Domain;

namespace PureTCOWebApp.Features.PushNotificationModule.Domain;

public class PushSubscription
{
#pragma warning disable
    public PushSubscription() { }
#pragma warning restore

    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>The push service endpoint URL (FCM, Mozilla, etc.)</summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>ECDH public key from the browser (base64url)</summary>
    public string P256dh { get; set; } = string.Empty;

    /// <summary>Auth secret from the browser (base64url)</summary>
    public string Auth { get; set; } = string.Empty;

    /// <summary>Optional UA string for debugging (which browser/device)</summary>
    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
