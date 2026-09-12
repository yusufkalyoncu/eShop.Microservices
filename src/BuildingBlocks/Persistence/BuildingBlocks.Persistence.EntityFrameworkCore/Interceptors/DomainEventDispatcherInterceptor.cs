using BuildingBlocks.Core.Domain;
using BuildingBlocks.Core.DomainEvents;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Persistence.EntityFrameworkCore.Interceptors;

public class DomainEventDispatcherInterceptor(IServiceProvider serviceProvider) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) return result;

        var aggregateRoots = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(x => x.DomainEvents)
            .ToList();

        if (domainEvents.Count == 0) return result;

        aggregateRoots.ForEach(x => x.ClearDomainEvents());

        var domainEventsDispatcher = serviceProvider.GetService<IDomainEventsDispatcher>();
        if (domainEventsDispatcher is not null)
        {
            await domainEventsDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        return result;
    }
}