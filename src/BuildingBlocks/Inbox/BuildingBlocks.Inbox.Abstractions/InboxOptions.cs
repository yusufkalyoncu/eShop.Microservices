namespace BuildingBlocks.Inbox.Abstractions;

public sealed class InboxOptions
{
    public int BatchSize { get; set; } = 100;
    public int MaxRetryCount { get; set; } = 5;
    public TimeSpan PollTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan LockTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxDegreeOfParallelism { get; set; } = 1;
    public TimeSpan ProcessedMessageRetention { get; set; } = TimeSpan.FromDays(14);
}