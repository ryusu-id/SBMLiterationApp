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

        // -------------------------------------------------------------------------
        // [MOCK DATA] — Used for PoC / dev testing only.
        // Replace with the actual code block below when going to production.
        // -------------------------------------------------------------------------
        var mockGroups = new List<(string GroupName, List<ParticipantEmailInfo> Participants)>
        {
            (
                "Alpha Team",
                [
                    new ParticipantEmailInfo("verio.renedominick@gmail.com",  "Verio Renedominick"),
                    new ParticipantEmailInfo("aberossyverio@gmail.com",        "Aber Ossy Verio"),
                ]
            ),
            (
                "Beta Team",
                [
                    new ParticipantEmailInfo("verio.renedominick@student.matanauniversity.ac.id", "Verio Renedominick (Student)"),
                    new ParticipantEmailInfo("verio.nobodyknows@gmail.com",                       "Verio Nobodyknows"),
                ]
            ),
        };

        var emailEvents = mockGroups.Select(g => new GroupAssignmentEmailRequestedEvent(
            AssignmentTitle: assignment.Title,
            AssignmentDescription: assignment.Description,
            DueDate: assignment.DueDate,
            GroupName: g.GroupName,
            Participants: g.Participants
        )).ToList();

        // -------------------------------------------------------------------------
        // [Actual Code] — Uncomment below and remove the mock block above for production.
        // Queries all groups that have at least one member and builds one outbox event per group.
        // -------------------------------------------------------------------------
        // var groups = await _dbContext.Groups
        //     .Include(g => g.Members)
        //         .ThenInclude(m => m.User)
        //     .Where(g => g.Members.Any())
        //     .ToListAsync(cancellationToken);
        //
        // if (groups.Count == 0)
        // {
        //     logger.LogWarning("[AssignmentEmail] No groups with members found. Skipping email notification.");
        //     return;
        // }
        //
        // var emailEvents = groups.Select(g => new GroupAssignmentEmailRequestedEvent(
        //     AssignmentTitle: assignment.Title,
        //     AssignmentDescription: assignment.Description,
        //     DueDate: assignment.DueDate,
        //     GroupName: g.Name,
        //     Participants: g.Members
        //         .Select(m => new ParticipantEmailInfo(m.User.Email!, m.User.Fullname))
        //         .ToList()
        // )).ToList();
        // -------------------------------------------------------------------------

        logger.LogInformation(
            "[AssignmentEmail] Queuing email notifications for assignment '{Title}' → {Count} group(s)",
            assignment.Title, emailEvents.Count);

        await outboxService.StoreManyAsync(emailEvents, cancellationToken);
    }
}
