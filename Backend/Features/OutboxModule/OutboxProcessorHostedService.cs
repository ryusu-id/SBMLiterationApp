using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.OutboxModule.Domain;

namespace PureTCOWebApp.Features.OutboxModule;

public class OutboxProcessorHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxSettings> options,
    ILogger<OutboxProcessorHostedService> logger) : BackgroundService
{
    private readonly OutboxSettings _settings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Outbox processor started. Interval: {Interval}s, MaxConcurrent: {MaxConcurrent}, MaxResilience: {MaxResilience}",
            _settings.ProcessingIntervalSeconds,
            _settings.MaxConcurrentProcessing,
            _settings.MaxResilienceCount);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in outbox processor loop.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_settings.ProcessingIntervalSeconds), stoppingToken);
        }

        logger.LogInformation("Outbox processor stopped.");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<DomainEventsDispatcher>();

        var pendingMessages = await dbContext.OutboxMessages
            .Where(m => m.RetryCount < m.MaxResilienceCount)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(stoppingToken);

        if (pendingMessages.Count == 0)
        {
            return;
        }

        logger.LogInformation("Processing {Count} outbox message(s).", pendingMessages.Count);

        using var semaphore = new SemaphoreSlim(_settings.MaxConcurrentProcessing, _settings.MaxConcurrentProcessing);

        var tasks = pendingMessages.Select(message => ProcessMessageAsync(
            message, dbContext, dispatcher, semaphore, stoppingToken));

        await Task.WhenAll(tasks);

        await dbContext.SaveChangesAsync(stoppingToken);
    }

    private async Task ProcessMessageAsync(
        OutboxMessage message,
        ApplicationDbContext dbContext,
        DomainEventsDispatcher dispatcher,
        SemaphoreSlim semaphore,
        CancellationToken stoppingToken)
    {
        await semaphore.WaitAsync(stoppingToken);
        try
        {
            var eventType = Type.GetType(message.EventType);
            if (eventType is null)
            {
                logger.LogError(
                    "Cannot resolve event type '{EventType}' for outbox message {Id}. Skipping.",
                    message.EventType, message.Id);
                message.RecordFailure($"Cannot resolve event type: {message.EventType}");
                return;
            }

            var domainEvent = (IDomainEvent?)JsonSerializer.Deserialize(message.Payload, eventType);
            if (domainEvent is null)
            {
                logger.LogError(
                    "Failed to deserialize payload for outbox message {Id} (type: {EventType}). Skipping.",
                    message.Id, message.EventType);
                message.RecordFailure("Deserialization returned null.");
                return;
            }

            await dispatcher.DispatchAsync([domainEvent], stoppingToken);

            dbContext.OutboxMessages.Remove(message);
            await dbContext.OutboxProcessedMessages.AddAsync(
                OutboxProcessedMessage.FromOutboxMessage(message), stoppingToken);

            logger.LogDebug("Outbox message {Id} ({EventType}) processed successfully.", message.Id, message.EventType);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to process outbox message {Id} ({EventType}). RetryCount: {RetryCount}/{MaxResilience}.",
                message.Id, message.EventType, message.RetryCount + 1, message.MaxResilienceCount);
            message.RecordFailure(ex.Message);
        }
        finally
        {
            semaphore.Release();
        }
    }
}
