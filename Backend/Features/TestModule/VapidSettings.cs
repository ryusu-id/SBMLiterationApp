namespace PureTCOWebApp.Features.TestModule;

public class VapidSettings
{
    public const string SectionName = "Vapid";

    /// <summary>The mailto: or https: contact URI included in VAPID JWTs.</summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>Base64-url encoded VAPID public key (P-256). Share this with the frontend.</summary>
    public string PublicKey { get; init; } = string.Empty;

    /// <summary>Base64-url encoded VAPID private key (P-256). Keep this secret.</summary>
    public string PrivateKey { get; init; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Subject) &&
        !string.IsNullOrWhiteSpace(PublicKey) &&
        !string.IsNullOrWhiteSpace(PrivateKey);
}
