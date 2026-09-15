using BuildingBlocks.Core.Options;
using BuildingBlocks.Observability;
using BuildingBlocks.Inbox.PostgreSql;
using BuildingBlocks.Messaging.MassTransit.Inbox;
using BuildingBlocks.Persistence.EntityFrameworkCore.Extensions;
using BuildingBlocks.Persistence.PostgreSql;
using BuildingBlocks.Web.Exceptions;
using BuildingBlocks.Web.OpenApi;
using Identity.Contracts.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Options;
using Notification.API.Application.Strategies;
using Notification.API.Infrastructure.Data;
using Notification.API.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability("Notification.API");

// Add Options with Validation
builder.Services.AddAppOptions(builder.Configuration, typeof(Program).Assembly);

// Add Global Exception Handler
builder.Services.AddGlobalExceptionHandler();

// Add API Versioning and OpenAPI
builder.Services.AddDocs();

// Add PostgreSQL DbContext
builder.Services.AddPostgresDbContext<NotificationDbContext>(interceptors: sp =>
    [sp.GetRequiredService<BuildingBlocks.Inbox.EntityFrameworkCore.InboxInsertInterceptor>()]);

// Add Inbox Pattern
builder.Services.AddPostgreSqlInbox<NotificationDbContext>();

// Register Strategy implementations
builder.Services.AddScoped<INotificationStrategy, EmailNotificationStrategy>();
builder.Services.AddScoped<INotificationStrategy, SmsNotificationStrategy>();
builder.Services.AddScoped<INotificationStrategy, PushNotificationStrategy>();
builder.Services.AddScoped<NotificationDispatcher>();

// Add MassTransit with RabbitMQ — scans for IIntegrationEventHandler<> implementations
// and auto-registers InboxMassTransitConsumer<T> for each event type
builder.Services.AddMassTransitEventBusWithInbox(
    [typeof(Program).Assembly, typeof(UserRegisteredIntegrationEvent).Assembly],
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

var app = builder.Build();

// Apply EF Core migrations on startup
app.Services.ApplyDatabaseMigrations<NotificationDbContext>();

// Use Global Exception Handler
app.UseGlobalExceptionHandler();

// Map OpenAPI endpoints and Scalar UI
if (app.Environment.IsDevelopment())
{
    app.UseDocs();
}

app.Run();