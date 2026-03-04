using PureTCOWebApp.Core.Events;

namespace PureTCOWebApp.Features.EmailModule.Events;

public record GroupAssignmentEmailRequestedEvent(
    string AssignmentTitle,
    string? AssignmentDescription,
    DateTime? DueDate,
    string GroupName,
    List<ParticipantEmailInfo> Participants
) : IDomainEvent;
