using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.UserXpModule.Domain.Events;

public class UserExpSnapshotEventHandler : IDomainEventHandler<UserExpCreatedEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public UserExpSnapshotEventHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UserExpCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var userExpEvent = domainEvent.UserExpEvent;

        var lastSnapshot = await _dbContext.UserExpSnapshots
            .AsNoTracking()
            .Where(e => e.UserId == userExpEvent.UserId)
            .OrderByDescending(s => s.SnapshotSeq)
            .FirstOrDefaultAsync(cancellationToken);

        var lastEventId = lastSnapshot?.LastEventSeq ?? 0;
        var unsnapshottedEventCount = await _dbContext.UserExpEvents
            .AsNoTracking()
            .Where(e => e.UserId == userExpEvent.UserId && e.Id > lastEventId)
            .OrderByDescending(e => e.Id)
            .CountAsync(cancellationToken);

        // Only create snapshot if we've exceeded the interval threshold
        if (unsnapshottedEventCount < ExpConstants.SNAPSHOT_EVENT_INTERVAL)
        {
            return;
        }

        var accumulatedExp = await _dbContext.UserExpEvents
            .AsNoTracking()
            .Where(e =>
                e.UserId == userExpEvent.UserId 
                && e.Id > lastEventId 
                && e.Id <= userExpEvent.Id)
            .SumAsync(e => e.Exp, cancellationToken);
        
        var newSnapshot = UserExpSnapshot.Create(
            userExpEvent.UserId,
            (lastSnapshot?.SnapshotSeq ?? 0) + 1,
            userExpEvent.Id,
            (lastSnapshot?.Exp ?? 0) + accumulatedExp
        );
        await _dbContext.UserExpSnapshots.AddAsync(newSnapshot, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
