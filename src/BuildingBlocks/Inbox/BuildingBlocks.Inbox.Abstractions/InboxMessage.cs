namespace BuildingBlocks.Inbox.Abstractions;

public sealed class InboxMessage
{
    public Guid Id { get; init; }
    public string Type { get; init; } = null!;
    public string Content { get; init; } = null!;
    public string? PartitionKey { get; init; }
    public DateTime OccurredOnUtc { get; init; }
    public DateTime ReceivedOnUtc { get; init; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public DateTime? NextAttemptAtUtc { get; private set; }
    public DateTime? LockedUntilUtc { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }
    public InboxMessageStatus Status { get; private set; } = InboxMessageStatus.Pending;

    private InboxMessage() { }

    public InboxMessage(Guid id, string type, string content, DateTime occurredOnUtc, string? partitionKey = null)
    {
        Id = id;
        Type = type;
        Content = content;
        PartitionKey = partitionKey;
        OccurredOnUtc = occurredOnUtc;
        ReceivedOnUtc = DateTime.UtcNow;
        RetryCount = 0;
        Status = InboxMessageStatus.Pending;
    }

    public void MarkProcessed(DateTime processedOnUtc)
    {
        Status = InboxMessageStatus.Processed;
        ProcessedOnUtc = processedOnUtc;
        Error = null;
        NextAttemptAtUtc = null;
        LockedUntilUtc = null;
    }

    public void MarkFailed(string error, int maxRetryCount, DateTime? nextAttemptAtUtc)
    {
        RetryCount++;
        Error = error;
        LockedUntilUtc = null;
        Status = RetryCount >= maxRetryCount ? InboxMessageStatus.DeadLettered : InboxMessageStatus.Pending;
        NextAttemptAtUtc = Status == InboxMessageStatus.Pending ? nextAttemptAtUtc : null;
    }

    public static InboxMessage Rehydrate(Guid id, string type, string content, DateTime occurredOnUtc, string? partitionKey, int retryCount)
    {
        var stub = new InboxMessage(id, type, content, occurredOnUtc, partitionKey)
        {
            RetryCount = retryCount
        };
        return stub;
    }
}