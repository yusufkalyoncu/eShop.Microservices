namespace BuildingBlocks.Outbox.Abstractions;

public sealed class OutboxMessage
{
    public Guid Id { get; init; }
    public string Type { get; init; } = null!;
    public string Content { get; init; } = null!;
    public DateTime OccurredOnUtc { get; init; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public DateTime? LockedUntilUtc { get; private set; }
    public DateTime? NextAttemptAtUtc { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }
    public OutboxMessageStatus Status { get; private set; } = OutboxMessageStatus.Pending;

    private OutboxMessage() { }

    public OutboxMessage(string type, string content)
    {
        Id = Guid.NewGuid();
        Type = type;
        Content = content;
        OccurredOnUtc = DateTime.UtcNow;
        RetryCount = 0;
        Status = OutboxMessageStatus.Pending;
    }
}