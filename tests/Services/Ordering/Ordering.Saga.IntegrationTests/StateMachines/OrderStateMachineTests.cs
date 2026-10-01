using FluentAssertions;
using Inventory.Contracts.IntegrationCommands;
using Inventory.Contracts.IntegrationEvents;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Contracts.IntegrationCommands;
using Ordering.Contracts.IntegrationEvents;
using Ordering.Saga.IntegrationTests.Infrastructure;
using Ordering.Saga.StateMachines;
using Payment.Contracts.IntegrationCommands;
using Payment.Contracts.IntegrationEvents;

namespace Ordering.Saga.IntegrationTests.StateMachines;

public class OrderStateMachineTests : IClassFixture<SagaApiFactory>
{
    private readonly ITestHarness _testHarness;
    private readonly ISagaStateMachineTestHarness<OrderStateMachine, OrderState> _sagaHarness;

    public OrderStateMachineTests(SagaApiFactory factory)
    {
        _testHarness = factory.Services.GetRequiredService<ITestHarness>();
        _sagaHarness = _testHarness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
    }

    [Fact]
    public async Task Scenario1_HappyPath_OrderToCompleted()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        // 1. Order Placed
        await _testHarness.Bus.Publish(new OrderPlacedIntegrationEvent
        {
            OrderId = orderId,
            UserName = "test_user",
            TotalAmount = 150m,
            PaymentToken = "tok_success",
            Items = new Dictionary<Guid, int> { { Guid.NewGuid(), 2 } }
        });

        // Assert Step 1
        (await _sagaHarness.Consumed.Any<OrderPlacedIntegrationEvent>()).Should().BeTrue();
        
        var sagaInstance = await _sagaHarness.Exists(orderId, x => x.StockReserving);
        sagaInstance.Should().NotBeNull();

        // Verify it published ReserveStockCommand
        (await _testHarness.Published.Any<ReserveStockCommand>()).Should().BeTrue();

        // 2. Stock Reserved
        await _testHarness.Bus.Publish(new StockReservedEvent { OrderId = orderId });

        // Assert Step 2
        (await _sagaHarness.Consumed.Any<StockReservedEvent>()).Should().BeTrue();
        
        sagaInstance = await _sagaHarness.Exists(orderId, x => x.PaymentProcessing);
        sagaInstance.Should().NotBeNull();

        // Verify it published ProcessPaymentIntegrationCommand
        (await _testHarness.Published.Any<ProcessPaymentIntegrationCommand>()).Should().BeTrue();

        // 3. Payment Succeeded
        await _testHarness.Bus.Publish(new PaymentSucceededEvent { OrderId = orderId });

        // Assert Step 3
        (await _sagaHarness.Consumed.Any<PaymentSucceededEvent>()).Should().BeTrue();
        
        sagaInstance = await _sagaHarness.Exists(orderId, x => x.Completed);
        sagaInstance.Should().NotBeNull();

        // Verify it published CompleteOrderCommand
        (await _testHarness.Published.Any<CompleteOrderCommand>()).Should().BeTrue();

    }

    [Fact]
    public async Task Scenario2_StockReservationFailed_OrderCancelled()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        // 1. Order Placed
        await _testHarness.Bus.Publish(new OrderPlacedIntegrationEvent
        {
            OrderId = orderId,
            UserName = "test_user",
            TotalAmount = 50m,
            PaymentToken = "tok_success",
            Items = new Dictionary<Guid, int>()
        });

        // 2. Stock Failed
        await _testHarness.Bus.Publish(new StockReservationFailedEvent { OrderId = orderId, Reason = "Out of stock" });

        // Assert
        (await _sagaHarness.Consumed.Any<StockReservationFailedEvent>()).Should().BeTrue();
        
        var sagaInstance = await _sagaHarness.Exists(orderId, x => x.Cancelled);
        sagaInstance.Should().NotBeNull();

        // Verify it published CancelOrderCommand
        (await _testHarness.Published.Any<CancelOrderCommand>()).Should().BeTrue();
    }

    [Fact]
    public async Task Scenario3_PaymentFailed_OrderCancelled_And_StockReleased()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        // 1. Order Placed
        await _testHarness.Bus.Publish(new OrderPlacedIntegrationEvent
        {
            OrderId = orderId,
            UserName = "test_user",
            TotalAmount = 999m,
            PaymentToken = "tok_fail",
            Items = new Dictionary<Guid, int> { { Guid.NewGuid(), 5 } }
        });

        // 2. Stock Reserved
        await _testHarness.Bus.Publish(new StockReservedEvent { OrderId = orderId });

        // 3. Payment Failed
        await _testHarness.Bus.Publish(new PaymentFailedEvent { OrderId = orderId, Reason = "Card declined" });

        // Assert
        (await _sagaHarness.Consumed.Any<PaymentFailedEvent>()).Should().BeTrue();
        
        var sagaInstance = await _sagaHarness.Exists(orderId, x => x.Cancelled);
        sagaInstance.Should().NotBeNull();

        // Verify Compensating Transaction (ReleaseStockCommand) was published
        (await _testHarness.Published.Any<ReleaseStockCommand>(x => x.Context != null && x.Context.Message.OrderId == orderId)).Should().BeTrue();
        
        // Verify CancelOrderCommand was published
        (await _testHarness.Published.Any<CancelOrderCommand>(x => x.Context != null && x.Context.Message.OrderId == orderId)).Should().BeTrue();
    }
}