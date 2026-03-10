using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.AssignmentModule.Domain.Events;
using PureTCOWebApp.Features.OutboxModule;

namespace PureTCOWebApp.Features.EmailModule.Events;

public class AssignmentCreatedEmailEventHandler(
    ApplicationDbContext dbContext,
    IOutboxService outboxService,
    ILogger<AssignmentCreatedEmailEventHandler> logger
) : IDomainEventHandler<AssignmentCreatedEvent>
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task Handle(AssignmentCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var assignment = domainEvent.Assignment;

        var groups = await _dbContext.Groups
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .Where(g => g.Members.Any())
            .ToListAsync(cancellationToken);
        
        if (groups.Count == 0)
        {
            logger.LogWarning("[AssignmentEmail] No groups with members found. Skipping email notification.");
            return;
        }
        
        var emailEvents = groups.Select(g => new GroupAssignmentEmailRequestedEvent(
            AssignmentTitle: assignment.Title,
            AssignmentDescription: assignment.Description,
            DueDate: assignment.DueDate,
            GroupName: g.Name,
            Participants: g.Members
                .Select(m => new ParticipantEmailInfo(m.User.Email!, m.User.Fullname))
                .ToList()
        )).ToList();

        logger.LogInformation(
            "[AssignmentEmail] Queuing email notifications for assignment '{Title}' → {Count} group(s)",
            assignment.Title, emailEvents.Count);

        await outboxService.StoreManyAsync(emailEvents, cancellationToken);
    }
}
