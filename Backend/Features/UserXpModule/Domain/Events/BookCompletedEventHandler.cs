using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain;
using PureTCOWebApp.Features.ReadingResourceModule.Domain.Events;

namespace PureTCOWebApp.Features.UserXpModule.Domain.Events;

public class BookCompletedEventHandler : IDomainEventHandler<BookCompletedEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public BookCompletedEventHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(BookCompletedEvent domainEvent, CancellationToken cancellationToken)
    {
        var report = domainEvent.Report;
        
        // Fetch resource to check if book is actually completed
        var resource = await _dbContext.Set<ReadingResourceBase>()
            .FirstOrDefaultAsync(r => r.Id == report.ReadingResourceId, cancellationToken);
        
        if (resource == null) return;
        
        // Check if book is completed
        if (report.CurrentPage < resource.Page || resource.Page == 0) return;
        
        // Check if ISBN exists in ReadingRecommendation
        var recommendation = await _dbContext.ReadingRecommendations
            .FirstOrDefaultAsync(r => r.ISBN == resource.ISBN, cancellationToken);
        
        if (recommendation != null)
        {
            // Use exp from recommendation
            var expEvent = UserExpEvent.Create(
                report.UserId, 
                recommendation.Exp, 
                "RecommendedBookCompleted", 
                report.Id
            );
            await _dbContext.UserExpEvents.AddAsync(expEvent, cancellationToken);
        }
        else
        {
            // Check last BookCompleted event
            var lastBookCompleted = await _dbContext.UserExpEvents
                .Where(e => e.UserId == report.UserId && e.EventName == "BookCompleted")
                .OrderByDescending(e => e.CreateTime)
                .FirstOrDefaultAsync(cancellationToken);
            
            // Give exp if last completion was 7+ days ago or never
            if (lastBookCompleted == null || 
                (DateTime.UtcNow - lastBookCompleted.CreateTime).TotalDays >= ExpConstants.BOOK_COMPLETION_COOLDOWN_DAYS)
            {
                var expEvent = UserExpEvent.Create(
                    report.UserId, 
                    ExpConstants.BOOK_COMPLETED, 
                    "BookCompleted", 
                    report.Id
                );
                await _dbContext.UserExpEvents.AddAsync(expEvent, cancellationToken);
            }
        }
    }
}
