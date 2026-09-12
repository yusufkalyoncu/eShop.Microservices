namespace BuildingBlocks.Inbox.Abstractions;

public interface IInboxSignal
{
    void Notify();
    Task WaitForSignalAsync(CancellationToken cancellationToken);
}