using BuildingBlocks.Messaging.MassTransit.Options;
using BuildingBlocks.Observability;
using BuildingBlocks.Persistence.EntityFrameworkCore.Extensions;
using BuildingBlocks.Persistence.PostgreSql;
using MassTransit;
using Microsoft.Extensions.Options;
using Ordering.Saga.Infrastructure;
using Ordering.Saga.StateMachines;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability("Ordering.Saga");

builder.Services.AddPostgresDbContext<SagaDbContext>();

builder.Services.AddOptions<RabbitMqOptions>().BindConfiguration(RabbitMqOptions.SectionName);

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .EntityFrameworkRepository(r =>
        {
            r.ExistingDbContext<SagaDbContext>();
            r.UsePostgres();
        });

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqOptions = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
        cfg.Host(rabbitMqOptions.Host, h =>
        {
            h.Username(rabbitMqOptions.Username);
            h.Password(rabbitMqOptions.Password);
        });

        // Use InMemoryOutbox to delay publishing until AFTER the saga state is saved to DB
        // This prevents the race condition where responses arrive before the DB commits
        cfg.UseInMemoryOutbox(context);

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();

// Automatically apply EF Core migrations
host.Services.ApplyDatabaseMigrations<SagaDbContext>();

await host.RunAsync();