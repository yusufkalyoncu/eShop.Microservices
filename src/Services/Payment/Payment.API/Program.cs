using BuildingBlocks.Application;
using BuildingBlocks.Observability;
using Payment.API.Infrastructure.Data;
using BuildingBlocks.Persistence.PostgreSql;
using BuildingBlocks.Web.Exceptions;
using BuildingBlocks.Persistence.EntityFrameworkCore.Extensions;
using BuildingBlocks.Messaging.MassTransit;
using BuildingBlocks.Messaging.MassTransit.Options;
using BuildingBlocks.Outbox.EntityFrameworkCore;
using BuildingBlocks.Outbox.PostgreSql;
using BuildingBlocks.Web.Security;
using MassTransit;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability("Payment.API");

// Add Database
builder.Services.AddPostgresDbContext<PaymentDbContext>(interceptors: sp =>
    [sp.GetRequiredService<OutboxInsertInterceptor>()]);

// Add Outbox Pattern
builder.Services.AddPostgreSqlOutbox<PaymentDbContext>();

// Add MassTransit with RabbitMQ
builder.Services.AddMassTransitEventBus(
    [typeof(Program).Assembly],
    configure =>
    {
        configure.UsingRabbitMq((ctx, cfg) =>
        {
            var rabbitMqOptions = ctx.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
            cfg.Host(rabbitMqOptions.Host, h =>
            {
                h.Username(rabbitMqOptions.Username);
                h.Password(rabbitMqOptions.Password);
            });
            cfg.ConfigureEndpoints(ctx);
        });
    });

// Add CQRS Handlers and Pipeline Behaviors (Logging, Validation) automatically using Scrutor
builder.Services.AddApplicationHandlers(typeof(Program).Assembly);

// Add Default Distributed Cache (In-Memory) for CachingDecorator
builder.Services.AddDistributedMemoryCache();

// Add Global Exception Handler
builder.Services.AddGlobalExceptionHandler();

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

// Automatically apply EF Core migrations
app.Services.ApplyDatabaseMigrations<PaymentDbContext>();

app.Run();