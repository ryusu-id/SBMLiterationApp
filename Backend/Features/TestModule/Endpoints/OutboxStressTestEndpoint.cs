using FastEndpoints;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.OutboxModule;
using PureTCOWebApp.Features.TestModule.Events;

namespace PureTCOWebApp.Features.TestModule.Endpoints;

public record OutboxStressTestRequest(int Count = 1000);

public record OutboxStressTestResponse(
    Guid BatchId,
    int EventsQueued,
    string Message
);

/// <summary>
/// Stress-test endpoint for the Outbox pattern.
/// Inserts <c>Count</c> OutboxStressTestEvents into outbox_messages atomically,
/// then lets OutboxProcessorHostedService pick them up in the background.
///
/// Watch the logs to observe the SemaphoreSlim(MaxConcurrentProcessing) throttling.
/// Route: POST /api/test-items/outbox-stress
/// </summary>
public class OutboxStressTestEndpoint(
    ApplicationDbContext dbContext,
    IOutboxService outboxService,
    ILogger<OutboxStressTestEndpoint> logger
) : Endpoint<OutboxStressTestRequest, ApiResponse<OutboxStressTestResponse>>
{
    public override void Configure()
    {
        Post("outbox-stress");
        Group<TestModuleEndpointGroup>();
        Roles("admin");
        Summary(s =>
        {
            s.Summary = "[Stress Test] Bulk-insert N events into the outbox.";
            s.Description =
                "Inserts Count (default 1000) OutboxStressTestEvents into outbox_messages in a single " +
                "transaction. The OutboxProcessorHostedService will then process them with " +
                "MaxConcurrentProcessing parallelism. Watch the logs to observe throughput.";
        });
    }

    public override async Task HandleAsync(OutboxStressTestRequest req, CancellationToken ct)
    {
        var count = Math.Max(1, Math.Min(req.Count, 10_000)); // clamp 1–10000
        var batchId = Guid.NewGuid();

        logger.LogInformation(
            "[OutboxStressTest] Queueing {Count} events for batch {BatchId}",
            count, batchId);

        var events = Enumerable.Range(1, count)
            .Select(i => new OutboxStressTestEvent(i, batchId))
            .ToList();

        await outboxService.StoreManyAsync(events, ct);

        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation(
            "[OutboxStressTest] {Count} events inserted into outbox_messages. BatchId: {BatchId}",
            count, batchId);

        var response = new OutboxStressTestResponse(
            BatchId: batchId,
            EventsQueued: count,
            Message: $"{count} events queued in outbox_messages. Monitor logs for processing progress."
        );

        await Send.OkAsync(Result.Success(response), cancellation: ct);
    }
}
