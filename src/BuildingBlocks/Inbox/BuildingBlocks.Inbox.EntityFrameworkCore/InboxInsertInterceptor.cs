using BuildingBlocks.Inbox.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Inbox.EntityFrameworkCore;

public sealed class InboxInsertInterceptor(IInboxSignal inboxSignal) : SaveChangesInterceptor
{
    private bool _hasNewInboxMessages;
    
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            _hasNewInboxMessages = eventData.Context.ChangeTracker
                .Entries<InboxMessage>()
                .Any(e => e.State == EntityState.Added);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
    
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, 
        int result, 
        CancellationToken cancellationToken = default)
    {
        var saveResult = await base.SavedChangesAsync(eventData, result, cancellationToken);
        
        if (saveResult > 0 && _hasNewInboxMessages)
        {
            inboxSignal.Notify();
            _hasNewInboxMessages = false;
        }

        return saveResult;
    }
}