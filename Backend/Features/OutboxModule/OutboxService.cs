using System.Text.Json;
using Microsoft.Extensions.Options;
using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.OutboxModule.Domain;

namespace PureTCOWebApp.Features.OutboxModule;

public class OutboxService(ApplicationDbContext dbContext, IOptions<OutboxSettings> options) : IOutboxService
{
    private readonly OutboxSettings _settings = options.Value;

    public async Task StoreManyAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var eventType = domainEvent.GetType().AssemblyQualifiedName
                ?? domainEvent.GetType().FullName
                ?? domainEvent.GetType().Name;

            var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());

            var message = OutboxMessage.Create(eventType, payload, _settings.MaxResilienceCount);

            await dbContext.OutboxMessages.AddAsync(message, cancellationToken);
        }
    }
}
