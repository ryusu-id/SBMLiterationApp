namespace PureTCOWebApp.Features.OutboxModule.Domain;

public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public int RetryCount { get; private set; }
    public int MaxResilienceCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastAttemptedAt { get; private set; }
    public string? LastError { get; private set; }

#pragma warning disable CS8618
    protected OutboxMessage() { }
#pragma warning restore CS8618

    public static OutboxMessage Create(string eventType, string payload, int maxResilienceCount)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Payload = payload,
            RetryCount = 0,
            MaxResilienceCount = maxResilienceCount,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void RecordFailure(string error)
    {
        RetryCount++;
        LastAttemptedAt = DateTime.UtcNow;
        LastError = error;
    }

    public bool HasExceededMaxResilience() => RetryCount >= MaxResilienceCount;
}
