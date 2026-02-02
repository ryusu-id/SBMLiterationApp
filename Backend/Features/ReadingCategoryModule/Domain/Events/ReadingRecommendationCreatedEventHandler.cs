using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingRecommendationModule.Domain.Events;

namespace PureTCOWebApp.Features.ReadingCategoryModule.Domain.Events;

public class ReadingRecommendationCreatedEventHandler : IDomainEventHandler<ReadingRecommendationCreatedEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public ReadingRecommendationCreatedEventHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(ReadingRecommendationCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var categoryName = domainEvent.Recommendation.ReadingCategory.Trim();

        var exists = await _dbContext.ReadingCategories
            .AnyAsync(c => c.CategoryName.ToLower() == categoryName.ToLower(), cancellationToken);

        if (!exists)
        {
            var category = ReadingCategory.Create(categoryName);
            await _dbContext.ReadingCategories.AddAsync(category, cancellationToken);
        }
    }
}
