using System.Reflection;

namespace Architecture.Tests;

public abstract class BaseArchitectureTest
{
    protected static readonly Assembly[] MicroserviceAssemblies =
    [
        typeof(Basket.API.Domain.Models.ShoppingCart).Assembly,
        typeof(Catalog.API.Features.Grpc.CatalogGrpcService).Assembly,
        typeof(Identity.API.Features.Auth.LoginUser.LoginUserCommand).Assembly,
        typeof(Inventory.API.Consumers.ReserveStockConsumer).Assembly,
        typeof(Notification.API.Application.Strategies.NotificationMessage).Assembly,
        typeof(Ordering.API.Domain.Models.Order).Assembly,
        typeof(Payment.API.Consumers.ProcessPaymentConsumer).Assembly
    ];
}