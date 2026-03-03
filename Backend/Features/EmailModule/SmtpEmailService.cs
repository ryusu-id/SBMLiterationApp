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
    public async Task SendGroupAssignmentNotificationAsync(GroupAssignmentEmailData data, CancellationToken ct = default)
    {
        var participants = data.Participants.ToList();

        if (participants.Count == 0)
        {
            logger.LogWarning("SendGroupAssignmentNotificationAsync called with no participants – skipping.");
            return;
        }

        using var smtp = new SmtpClient();

        try
        {
            logger.LogInformation(
                "Connecting to SMTP server {Host}:{Port} with user {User}",
                _settings.SmtpHost, _settings.SmtpPort, _settings.SenderEmail);
            await smtp.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls, ct);
            logger.LogInformation("Connected to SMTP server. Authenticating...");
            await smtp.AuthenticateAsync(_settings.SenderEmail, _settings.AppPassword, ct);

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

                await smtp.SendAsync(message, ct);
            }
        }
        finally
        {
            await smtp.DisconnectAsync(true, ct);
        }
    }
}
