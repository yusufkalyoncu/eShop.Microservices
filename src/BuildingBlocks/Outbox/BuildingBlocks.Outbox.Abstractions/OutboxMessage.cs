namespace BuildingBlocks.Outbox.Abstractions;

public sealed class OutboxMessage
{
    public Guid Id { get; init; }
    public string Type { get; init; } = null!;
    public string Content { get; init; } = null!;
    public string? PartitionKey { get; init; }
    public DateTime OccurredOnUtc { get; init; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public DateTime? LockedUntilUtc { get; private set; }
    public DateTime? NextAttemptAtUtc { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }
    public OutboxMessageStatus Status { get; private set; } = OutboxMessageStatus.Pending;
    
    /// <summary>
    /// W3C traceparent of the originating HTTP request.
    /// Stored so the OutboxProcessor can restore the trace context when publishing,
    /// linking the async outbox publish span back to the original request trace.
    /// </summary>
    public string? TraceParent { get; init; }

    private OutboxMessage() { }

    public OutboxMessage(string type, string content, string? partitionKey = null, string? traceParent = null)
    {
        Id = Guid.NewGuid();
        Type = type;
        Content = content;
        PartitionKey = partitionKey;
        OccurredOnUtc = DateTime.UtcNow;
        RetryCount = 0;
        Status = OutboxMessageStatus.Pending;
        TraceParent = traceParent;
    }
}