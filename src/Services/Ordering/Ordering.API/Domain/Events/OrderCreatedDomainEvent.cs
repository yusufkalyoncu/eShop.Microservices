using BuildingBlocks.Core.DomainEvents;
using Ordering.API.Domain.Models;

namespace Ordering.API.Domain.Events;

public record OrderCreatedDomainEvent(Order Order) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => nameof(OrderCreatedDomainEvent);
}