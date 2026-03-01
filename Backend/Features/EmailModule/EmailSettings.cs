namespace PureTCOWebApp.Features.EmailModule;

public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string AppPassword { get; set; } = string.Empty;

    /// <summary>
    /// PoC / dev override: when set, ALL outgoing emails are redirected to this address
    /// instead of the actual participant emails. Remove or leave empty in production.
    /// </summary>
    public string? TestRecipientEmail { get; set; }

    /// <summary>
    /// Display name paired with <see cref="TestRecipientEmail"/>.
    /// </summary>
    public string? TestRecipientName { get; set; }
}
