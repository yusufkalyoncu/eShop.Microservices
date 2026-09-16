using BuildingBlocks.Core.DomainEvents;
using Catalog.API.Domain.ValueObjects;

namespace Catalog.API.Domain.Events;

public sealed record ProductPriceChangedDomainEvent(
    Guid ProductId,
    Money NewPrice) : IDomainEvent;