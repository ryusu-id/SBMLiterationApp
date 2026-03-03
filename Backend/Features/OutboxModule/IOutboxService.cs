using PureTCOWebApp.Core.Events;

namespace PureTCOWebApp.Features.OutboxModule;

public interface IOutboxService
{
    /// <summary>
    /// Serializes the domain events and stores them as outbox messages.
    /// Must be called within an active database transaction.
    /// </summary>
    Task StoreManyAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
