using PureTCOWebApp.Core.Events;

namespace PureTCOWebApp.Features.EmailModule.Events;

/// <summary>
/// Outbox event that carries all data required to send one group assignment
/// notification email blast. Stored as JSON in outbox_messages and picked up
/// by GroupAssignmentEmailRequestedEventHandler via OutboxProcessorHostedService.
///
/// Uses flat primitives only — no EF entity references — so it can be
/// safely serialized / deserialized by the Outbox processor.
/// </summary>
public record GroupAssignmentEmailRequestedEvent(
    string AssignmentTitle,
    string? AssignmentDescription,
    DateTime? DueDate,
    string GroupName,
    List<ParticipantEmailInfo> Participants
) : IDomainEvent;
