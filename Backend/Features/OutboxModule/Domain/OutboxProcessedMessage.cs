namespace PureTCOWebApp.Features.OutboxModule.Domain;

public class OutboxProcessedMessage
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime ProcessedAt { get; private set; }

#pragma warning disable CS8618
    protected OutboxProcessedMessage() { }
#pragma warning restore CS8618

    public static OutboxProcessedMessage FromOutboxMessage(OutboxMessage message)
    {
        return new OutboxProcessedMessage
        {
            Id = message.Id,
            EventType = message.EventType,
            Payload = message.Payload,
            CreatedAt = message.CreatedAt,
            ProcessedAt = DateTime.UtcNow
        };
    }
}
