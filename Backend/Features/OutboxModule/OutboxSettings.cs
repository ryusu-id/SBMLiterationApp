namespace PureTCOWebApp.Features.OutboxModule;

public class OutboxSettings
{
    public const string SectionName = "Outbox";

    /// <summary>
    /// Maximum number of retry attempts before a message is considered permanently failed.
    /// </summary>
    public int MaxResilienceCount { get; set; } = 5;

    /// <summary>
    /// Interval in seconds between each outbox processing run.
    /// </summary>
    public int ProcessingIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Maximum number of outbox messages to process concurrently per run.
    /// Tuned for a 4-core / 2 GB RAM server.
    /// </summary>
    public int MaxConcurrentProcessing { get; set; } = 4;
}
