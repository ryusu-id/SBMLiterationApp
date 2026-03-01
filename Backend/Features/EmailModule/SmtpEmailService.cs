using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace PureTCOWebApp.Features.EmailModule;

public class SmtpEmailService(
    IOptions<EmailSettings> options,
    ILogger<SmtpEmailService> logger
) : IEmailService
{
    private readonly EmailSettings _settings = options.Value;

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public async Task SendGroupAssignmentNotificationAsync(GroupAssignmentEmailData data, CancellationToken ct = default)
    {
        var participants = data.Participants.ToList();

        if (participants.Count == 0)
        {
            logger.LogWarning("SendGroupAssignmentNotificationAsync called with no participants – skipping.");
            return;
        }

        // Send a personalised email to each participant individually.
        // In this PoC the participant list is hardcoded to the developer's own
        // email addresses, so no real users are ever contacted.
        foreach (var participant in participants)
        {
            logger.LogInformation(
                "[PoC] Sending group-assignment email to {Email} ({Name})",
                participant.Email, participant.FullName);

            var message = GroupAssignmentEmailTemplate.Build(
                data,
                participant,
                _settings.SenderEmail,
                _settings.SenderName);

            await SendAsync(message, ct);
        }
    }

    public async Task SendTestEmailAsync(string? recipientEmail = null, string? recipientName = null, CancellationToken ct = default)
    {
        var toEmail = recipientEmail ?? _settings.TestRecipientEmail
            ?? throw new InvalidOperationException(
                "No recipient email provided and Email:TestRecipientEmail is not configured.");

        var toName = recipientName ?? _settings.TestRecipientName ?? toEmail;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = "[Test] SBM Literation – SMTP connectivity check";

        message.Body = new TextPart("html")
        {
            Text = $"""
                    <html>
                    <body style="font-family:Arial,sans-serif;color:#333;max-width:600px;margin:auto;padding:24px">
                      <h2 style="color:#2b6cb0">SBM Literation – Test Email</h2>
                      <p>Hi <strong>{toName}</strong>,</p>
                      <p>This is a test email to confirm that the SMTP configuration is working correctly.</p>
                      <p>If you received this message, the email service is ready.</p>
                      <hr style="border:none;border-top:1px solid #eee;margin:24px 0"/>
                      <p style="font-size:12px;color:#999">
                        Sent at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
                      </p>
                    </body>
                    </html>
                    """
        };

        await SendAsync(message, ct);
        logger.LogInformation("Test email sent successfully to {Recipient}", toEmail);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private async Task SendAsync(MimeMessage message, CancellationToken ct)
    {
        using var smtp = new SmtpClient();

        try
        {
            await smtp.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls, ct);
            await smtp.AuthenticateAsync(_settings.SenderEmail, _settings.AppPassword, ct);
            await smtp.SendAsync(message, ct);
        }
        finally
        {
            await smtp.DisconnectAsync(true, ct);
        }
    }
}
