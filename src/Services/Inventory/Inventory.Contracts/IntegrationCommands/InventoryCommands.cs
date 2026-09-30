using BuildingBlocks.Messaging.Abstractions;

namespace Inventory.Contracts.IntegrationCommands;

public class ReserveStockCommand : IIntegrationEvent // Commands are often modeled as IntegrationEvents in MassTransit since they use the same bus
{
    public Guid EventId { get; } = Guid.NewGuid();
    public static string EventName => "ReserveStockCommand";
    
    public Guid OrderId { get; init; }
    public Dictionary<Guid, int> Items { get; init; } = new();
}

public class ReleaseStockCommand : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public static string EventName => "ReleaseStockCommand";
    public Guid OrderId { get; init; }
    public Dictionary<Guid, int> Items { get; init; } = new();
}