using PureTCOWebApp.Core.Events;

namespace PureTCOWebApp.Features.UserXpModule.Domain.Events;

public record UserExpCreatedEvent(UserExpEvent UserExpEvent) : IDomainEvent;
