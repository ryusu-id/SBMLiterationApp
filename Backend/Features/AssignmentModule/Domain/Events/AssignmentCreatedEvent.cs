using PureTCOWebApp.Core.Events;

namespace PureTCOWebApp.Features.AssignmentModule.Domain.Events;

public record AssignmentCreatedEvent(Assignment Assignment) : IDomainEvent;
