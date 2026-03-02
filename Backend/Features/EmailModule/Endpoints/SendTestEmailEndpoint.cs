using FastEndpoints;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Features.EmailModule.Endpoints;

// ---------------------------------------------------------------------------
// Request / Response
// ---------------------------------------------------------------------------

/// <param name="RecipientEmail">
/// Optional override for the recipient email address.
/// If omitted, the value from <c>Email:TestRecipientEmail</c> in appsettings is used.
/// </param>
/// <param name="RecipientName">Optional display name for the recipient.</param>
public record SendTestEmailRequest(string? RecipientEmail, string? RecipientName);

public record SendTestEmailResponse(string Message, string SentTo);

// ---------------------------------------------------------------------------
// Endpoint
// ---------------------------------------------------------------------------

/// <summary>
/// Admin-only endpoint to verify that the SMTP service is configured
/// and can successfully deliver email. Safe to call at any time – sends
/// only to the address supplied in the request (or the configured test address).
/// </summary>
public class SendTestEmailEndpoint(IEmailService emailService, IConfiguration config)
    : Endpoint<SendTestEmailRequest, ApiResponse<SendTestEmailResponse>>
{
    public override void Configure()
    {
        Post("send-test");
        Group<EmailModuleEndpointGroup>();
        Roles("admin");
        Summary(s =>
        {
            s.Summary = "Send a test email to verify SMTP connectivity.";
            s.Description = "Sends a simple test email to Email:TestRecipientEmail configured in appsettings. No auth required (PoC only).";
        });
    }

    public override async Task HandleAsync(SendTestEmailRequest req, CancellationToken ct)
    {
        var configuredTestEmail = config["Email:TestRecipientEmail"];
        var resolvedEmail = req.RecipientEmail ?? configuredTestEmail;

        if (string.IsNullOrWhiteSpace(resolvedEmail))
        {
            Result noRecipient = new Error("EMAIL_NO_RECIPIENT", "No recipient email was provided and Email:TestRecipientEmail is not configured.");
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(noRecipient));
            return;
        }

        try
        {
            await emailService.SendTestEmailAsync(req.RecipientEmail, req.RecipientName, ct);

            await Send.OkAsync(Result.Success(
                new SendTestEmailResponse(
                    "Test email sent successfully.",
                    resolvedEmail)),
                cancellation: ct);
        }
        catch (Exception ex)
        {
            Result sendFailed = new Error("EMAIL_SEND_FAILED", ex.Message);
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(sendFailed));
        }
    }
}
