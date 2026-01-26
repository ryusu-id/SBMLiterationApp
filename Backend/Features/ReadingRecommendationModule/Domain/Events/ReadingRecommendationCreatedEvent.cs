using PureTCOWebApp.Core.Events;

namespace PureTCOWebApp.Features.ReadingRecommendationModule.Domain.Events;

public record ReadingRecommendationCreatedEvent(ReadingRecommendation Recommendation) : IDomainEvent;
