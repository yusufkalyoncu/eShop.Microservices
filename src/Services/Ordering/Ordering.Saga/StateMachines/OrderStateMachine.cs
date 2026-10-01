using Inventory.Contracts.IntegrationCommands;
using Inventory.Contracts.IntegrationEvents;
using MassTransit;
using Ordering.Contracts.IntegrationEvents;
using Payment.Contracts.IntegrationCommands;
using Payment.Contracts.IntegrationEvents;

namespace Ordering.Saga.StateMachines;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State StockReserving { get; private set; } = null!;
    public State PaymentProcessing { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Cancelled { get; private set; } = null!;

    public Event<OrderPlacedIntegrationEvent> OrderPlacedEvent { get; private set; } = null!;
    public Event<StockReservedEvent> StockReservedEvent { get; private set; } = null!;
    public Event<StockReservationFailedEvent> StockReservationFailedEvent { get; private set; } = null!;
    public Event<PaymentSucceededEvent> PaymentSucceededEvent { get; private set; } = null!;
    public Event<PaymentFailedEvent> PaymentFailedEvent { get; private set; } = null!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        // Correlate the incoming events with the Saga instance by OrderId
        Event(() => OrderPlacedEvent, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => StockReservedEvent, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => StockReservationFailedEvent, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => PaymentSucceededEvent, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => PaymentFailedEvent, x => x.CorrelateById(context => context.Message.OrderId));

        Initially(
            When(OrderPlacedEvent)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.UserName = context.Message.UserName;
                    context.Saga.TotalAmount = context.Message.TotalAmount;
                    context.Saga.PaymentToken = context.Message.PaymentToken;
                    context.Saga.Items = context.Message.Items; // Store items in saga state
                    context.Saga.CreatedAt = DateTime.UtcNow;
                })
                .Publish(context => new ReserveStockCommand
                {
                    OrderId = context.Saga.OrderId,
                    Items = context.Message.Items
                })
                .TransitionTo(StockReserving)
        );

        During(StockReserving,
            When(StockReservedEvent)
                .Then(context => context.Saga.UpdatedAt = DateTime.UtcNow)
                .Publish(context => new ProcessPaymentIntegrationCommand
                {
                    OrderId = context.Saga.OrderId,
                    Amount = context.Saga.TotalAmount,
                    PaymentToken = context.Saga.PaymentToken
                })
                .TransitionTo(PaymentProcessing),

            When(StockReservationFailedEvent)
                .Then(context => context.Saga.UpdatedAt = DateTime.UtcNow)
                .Publish(context => new Ordering.Contracts.IntegrationCommands.CancelOrderCommand
                {
                    OrderId = context.Saga.OrderId,
                    Reason = "Stock Reservation Failed: " + context.Message.Reason
                })
                .TransitionTo(Cancelled)
        );

        During(PaymentProcessing,
            When(PaymentSucceededEvent)
                .Then(context => context.Saga.UpdatedAt = DateTime.UtcNow)
                .Publish(context => new Ordering.Contracts.IntegrationCommands.CompleteOrderCommand
                {
                    OrderId = context.Saga.OrderId
                })
                .TransitionTo(Completed),

            When(PaymentFailedEvent)
                .Then(context => context.Saga.UpdatedAt = DateTime.UtcNow)
                // Compensating Transaction: Release Stock
                .Publish(context => new ReleaseStockCommand
                {
                    OrderId = context.Saga.OrderId,
                    Items = context.Saga.Items
                })
                .Publish(context => new Ordering.Contracts.IntegrationCommands.CancelOrderCommand
                {
                    OrderId = context.Saga.OrderId,
                    Reason = "Payment Failed: " + context.Message.Reason
                })
                .TransitionTo(Cancelled)
        );
    }
}