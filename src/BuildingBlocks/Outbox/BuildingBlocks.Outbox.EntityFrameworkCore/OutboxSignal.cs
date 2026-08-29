using BuildingBlocks.Outbox.Abstractions;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Outbox.EntityFrameworkCore;

public sealed class OutboxSignal(IOptions<OutboxOptions> options) : IOutboxSignal
{
    private readonly SemaphoreSlim _signal = new(0, 1);
    private readonly TimeSpan _pollTimeout = options.Value.PollTimeout;

    public void Notify()
    {
        try 
        {
            if (_signal.CurrentCount == 0)
            {
                _signal.Release();
            }
        }
        catch (SemaphoreFullException) 
        {
        }
    }

    public async Task WaitForSignalAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(_pollTimeout, cancellationToken);
    }
}