using PureTCOWebApp.Core.Events;

namespace PureTCOWebApp.Features.TestModule.Events;

/// <summary>
/// Lightweight dummy event used to stress-test the Outbox processor pipeline.
/// Carries only an index and a batch ID so we can correlate logs.
/// </summary>
public record OutboxStressTestEvent(int Index, Guid BatchId) : IDomainEvent;

/// <summary>
/// No-op handler — just logs that the event was picked up and processed by the
/// OutboxProcessorHostedService. No DB writes, no external calls.
/// </summary>
public class OutboxStressTestEventHandler(
    ILogger<OutboxStressTestEventHandler> logger
) : IDomainEventHandler<OutboxStressTestEvent>
{
    public Task Handle(OutboxStressTestEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[OutboxStressTest] Processed event #{Index} from batch {BatchId}",
            domainEvent.Index,
            domainEvent.BatchId);

        return Task.CompletedTask;
    }
}
