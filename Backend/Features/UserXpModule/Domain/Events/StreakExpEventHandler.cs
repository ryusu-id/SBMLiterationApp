using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.StreakModule.Domain.Events;

namespace PureTCOWebApp.Features.UserXpModule.Domain.Events;

public class StreakExpEventHandler : IDomainEventHandler<StreakLogCreatedEvent>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserExpDomainService _userExpService;

    public StreakExpEventHandler(ApplicationDbContext dbContext, UserExpDomainService userExpService)
    {
        _dbContext = dbContext;
        _userExpService = userExpService;
    }

    public async Task Handle(StreakLogCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var streakLog = domainEvent.StreakLog;
        
        // Get all streak logs for this user up to and including the current date
        var allStreakLogs = await _dbContext.StreakLogs
            .Where(s => s.UserId == streakLog.UserId && s.StreakDate <= streakLog.StreakDate)
            .OrderByDescending(s => s.StreakDate)
            .ToListAsync(cancellationToken);
        
        // Calculate the current consecutive streak count
        int consecutiveStreak = 0;
        DateOnly expectedDate = streakLog.StreakDate;
        
        foreach (var log in allStreakLogs)
        {
            if (log.StreakDate == expectedDate)
            {
                consecutiveStreak++;
                expectedDate = expectedDate.AddDays(-1);
            }
            else
            {
                break; // Streak is broken
            }
        }
        
        // Only award bonus if streak count is exactly a multiple of 7 (7th, 14th, 21st day, etc.)
        if (consecutiveStreak % ExpConstants.STREAK_BONUS_DAYS != 0) return;
        
        // Check if we already awarded XP for this streak milestone (using current streak log as reference)
        var alreadyGivenXp = await _dbContext.UserExpEvents
            .AnyAsync(e => e.UserId == streakLog.UserId &&
                          e.EventName == nameof(UserExpEvent.ExpEventType.StreakExp) &&
                          e.RefId == streakLog.Id, cancellationToken);
        
        if (alreadyGivenXp) return;
        
        var userExp = UserExpEvent.Create(
            streakLog.UserId,
            ExpConstants.STREAK_7_DAYS_BONUS,
            nameof(UserExpEvent.ExpEventType.StreakExp),
            streakLog.Id
        );
        
        await _dbContext.UserExpEvents.AddAsync(userExp, cancellationToken);
    }
}
