using FastEndpoints;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Features.EmailModule.Endpoints;

public record SendGroupAssignmentPreviewResponse(
    string Message,
    string GroupName,
    List<string> Participants
);

/// <summary>
/// PoC endpoint - fires a mock group-assignment notification with hardcoded data.
/// No request body. Recipient is always Email:TestRecipientEmail from appsettings.
/// </summary>
public class SendGroupAssignmentPreviewEndpoint(
    IEmailService emailService
) : EndpointWithoutRequest<ApiResponse<SendGroupAssignmentPreviewResponse>>
{
    public override void Configure()
    {
        Post("group-assignment-preview");
        Group<EmailModuleEndpointGroup>();
        Roles("admin");
        Summary(s =>
        {
            s.Summary = "[PoC] Send a mock group-assignment notification email.";
            s.Description = "No body needed. All data is mocked. Sends to Email:TestRecipientEmail in appsettings.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Hardcoded mock assignment - simulates a real group assignment being created
        // Both participants are the developer's own accounts (PoC only)
        var mockData = new GroupAssignmentEmailData(
            AssignmentTitle: "Week 3 Case Study - Supply Chain Strategy",
            AssignmentDescription: "Baca Chapter 5 - Strategic Operations dan submit refleksi singkat (min. 300 kata) melalui platform sebelum deadline.",
            DueDate: DateTime.UtcNow.AddDays(7),
            GroupName: "Alpha Group",
            Participants:
            [
                new ParticipantEmailInfo("your-test-recipient@gmail.com",  "Your Name"),
                new ParticipantEmailInfo("your-sender-email@gmail.com",    "Your Sender Account")
            ]
        );

        try
        {
            await emailService.SendGroupAssignmentNotificationAsync(mockData, ct);

            await Send.OkAsync(Result.Success(
                new SendGroupAssignmentPreviewResponse(
                    "[PoC] Mock group assignment notification sent successfully.",
                    mockData.GroupName,
                    mockData.Participants.Select(p => $"{p.FullName} <{p.Email}>").ToList())),
                cancellation: ct);
        }
        catch (Exception ex)
        {
            Result sendFailed = new Error("EMAIL_SEND_FAILED", ex.Message);
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(sendFailed));
        }
    }
}
