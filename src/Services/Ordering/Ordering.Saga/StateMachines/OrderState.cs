using MassTransit;

namespace Ordering.Saga.StateMachines;

public class OrderState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    
    // Concurrency check for EF Core
    public int Version { get; set; }
    
    // The current state of the state machine (stored as a string in DB)
    public string CurrentState { get; init; } = string.Empty;

    // Data saved during the saga to use in subsequent steps
    public Guid OrderId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string PaymentToken { get; set; } = string.Empty;
    
    // Ordered items stored to release stock in compensation
    public Dictionary<Guid, int> Items { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}