namespace BuildingBlocks.Outbox.Abstractions;

public sealed class OutboxOptions
{
    /// <summary>The maximum number of messages to read in a single batch.</summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>The maximum number of retry attempts. Messages reaching this count will no longer be processed.</summary>
    public int MaxRetryCount { get; set; } = 5;

    /// <summary>The maximum duration the processor will wait for a new signal before checking for messages again.</summary>
    public TimeSpan PollTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>The maximum duration a message can be locked for processing. If the lock expires, the message will be available for processing again.</summary>
    public TimeSpan LockTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>The maximum number of concurrent publish operations within a batch.</summary>
    public int MaxDegreeOfParallelism { get; set; } = 1;
}