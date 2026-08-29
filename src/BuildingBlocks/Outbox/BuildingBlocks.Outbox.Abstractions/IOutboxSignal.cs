namespace BuildingBlocks.Outbox.Abstractions;

public interface IOutboxSignal
{
    void Notify();
    Task WaitForSignalAsync(CancellationToken cancellationToken);
}