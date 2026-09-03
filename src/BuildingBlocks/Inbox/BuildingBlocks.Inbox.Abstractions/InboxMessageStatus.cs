namespace BuildingBlocks.Inbox.Abstractions;

public enum InboxMessageStatus
{
    Pending = 0,
    Processed = 1,
    DeadLettered = 2
}