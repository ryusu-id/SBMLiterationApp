using PureTCOWebApp.Core.Events;

namespace PureTCOWebApp.Features.EmailModule.Events;

/// <summary>
/// Handles GroupAssignmentEmailRequestedEvent dispatched by OutboxProcessorHostedService.
/// This is the SMTP handler — it runs in the background job, fully decoupled from the HTTP request.
/// On failure, the Outbox processor will retry up to MaxResilienceCount times.
/// </summary>
public class GroupAssignmentEmailRequestedEventHandler(
    IEmailService emailService,
    ILogger<GroupAssignmentEmailRequestedEventHandler> logger
) : IDomainEventHandler<GroupAssignmentEmailRequestedEvent>
{
    public async Task Handle(GroupAssignmentEmailRequestedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[AssignmentEmail] Sending assignment notification for '{Title}' to group '{Group}' ({Count} participants)",
            domainEvent.AssignmentTitle,
            domainEvent.GroupName,
            domainEvent.Participants.Count);

        var data = new GroupAssignmentEmailData(
            AssignmentTitle: domainEvent.AssignmentTitle,
            AssignmentDescription: domainEvent.AssignmentDescription,
            DueDate: domainEvent.DueDate,
            GroupName: domainEvent.GroupName,
            Participants: domainEvent.Participants
        );

        await emailService.SendGroupAssignmentNotificationAsync(data, cancellationToken);

        logger.LogInformation(
            "[AssignmentEmail] Notification sent successfully for group '{Group}'",
            domainEvent.GroupName);
    }
}
