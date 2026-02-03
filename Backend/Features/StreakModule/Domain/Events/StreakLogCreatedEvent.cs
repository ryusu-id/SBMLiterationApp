using PureTCOWebApp.Core.Events;

namespace PureTCOWebApp.Features.StreakModule.Domain.Events;

public record StreakLogCreatedEvent(StreakLog StreakLog) : IDomainEvent;
