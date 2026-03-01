namespace PureTCOWebApp.Features.EmailModule;

/// <summary>
/// Represents a single participant recipient for group assignment notification emails.
/// </summary>
/// <param name="Email">Participant's email address.</param>
/// <param name="FullName">Participant's full name (used in the email greeting).</param>
public record ParticipantEmailInfo(string Email, string FullName);

/// <summary>
/// Data required to send a group assignment notification.
/// </summary>
public record GroupAssignmentEmailData(
    string AssignmentTitle,
    string? AssignmentDescription,
    DateTime? DueDate,
    string GroupName,

    /// <summary>
    /// The list of participants in the group.
    /// In production this drives the blast-email recipients.
    /// In PoC mode (TestRecipientEmail set), all sends are redirected to the test address.
    /// </summary>
    IEnumerable<ParticipantEmailInfo> Participants
);

public interface IEmailService
{
    /// <summary>
    /// Sends a group assignment notification to all participants in the assignment data.
    /// When <c>Email:TestRecipientEmail</c> is configured, every message is redirected
    /// to that address for safe PoC testing.
    /// </summary>
    Task SendGroupAssignmentNotificationAsync(GroupAssignmentEmailData data, CancellationToken ct = default);

    /// <summary>
    /// Sends a generic test email to verify SMTP connectivity.
    /// Falls back to <c>Email:TestRecipientEmail</c> when no explicit recipient is supplied.
    /// </summary>
    Task SendTestEmailAsync(string? recipientEmail = null, string? recipientName = null, CancellationToken ct = default);
}
