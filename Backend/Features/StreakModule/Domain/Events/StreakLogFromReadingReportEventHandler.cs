using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain.Events;

namespace PureTCOWebApp.Features.StreakModule.Domain.Events;

public class StreakLogFromReadingReportEventHandler : IDomainEventHandler<ReadingReportCreatedEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public StreakLogFromReadingReportEventHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(ReadingReportCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var report = domainEvent.Report;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var existsToday = await _dbContext.StreakLogs
            .AnyAsync(s => s.UserId == report.UserId && s.StreakDate == today, cancellationToken);
        
        if (existsToday) return;
        
        var streakLog = StreakLog.Create(report.UserId, today);
        
        await _dbContext.StreakLogs.AddAsync(streakLog, cancellationToken);
    }
}
