using BuildingBlocks.Core.DomainEvents;

namespace BuildingBlocks.Core.Domain;

public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}