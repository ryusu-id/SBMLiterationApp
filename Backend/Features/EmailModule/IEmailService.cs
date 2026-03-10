namespace PureTCOWebApp.Features.EmailModule;

public record ParticipantEmailInfo(string Email, string FullName);

public record GroupAssignmentEmailData(
    string AssignmentTitle,
    string? AssignmentDescription,
    DateTime? DueDate,
    string GroupName,

    IEnumerable<ParticipantEmailInfo> Participants
);

public interface IEmailService
{
    Task SendGroupAssignmentNotificationAsync(GroupAssignmentEmailData data, CancellationToken ct = default);
}
