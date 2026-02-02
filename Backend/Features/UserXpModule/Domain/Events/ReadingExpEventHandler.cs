using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain.Events;

namespace PureTCOWebApp.Features.UserXpModule.Domain.Events;

public class ReadingExpEventHandler : IDomainEventHandler<ReadingReportCreatedEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public ReadingExpEventHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(ReadingReportCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var report = domainEvent.Report;
        
        var exp = report.CurrentPage * ExpConstants.READING_PER_PAGE;
        
        var userExp = UserExpEvent.Create(
            report.UserId,
            exp,
            nameof(UserExpEvent.ExpEventType.ReadingExp),
            report.Id
        );
        
        await _dbContext.UserExpEvents.AddAsync(userExp, cancellationToken);
    }
}
