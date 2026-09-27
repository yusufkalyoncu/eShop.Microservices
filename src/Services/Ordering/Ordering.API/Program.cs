using BuildingBlocks.Application;
using BuildingBlocks.Observability;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Persistence.PostgreSql;
using BuildingBlocks.Web.OpenApi;
using BuildingBlocks.Web.Exceptions;
using BuildingBlocks.Persistence.EntityFrameworkCore.Extensions;
using BuildingBlocks.Messaging.MassTransit.Inbox;
using BuildingBlocks.Messaging.MassTransit.Options;
using BuildingBlocks.Outbox.EntityFrameworkCore;
using BuildingBlocks.Outbox.PostgreSql;
using BuildingBlocks.Inbox.EntityFrameworkCore;
using BuildingBlocks.Inbox.PostgreSql;
using BuildingBlocks.Web.Security;
using MassTransit;
using Microsoft.Extensions.Options;
using Ordering.API.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability("Ordering.API");

builder.Services.AddPostgresDbContext<OrderingDbContext>(interceptors: sp =>
    [
        sp.GetRequiredService<OutboxInsertInterceptor>(),
        sp.GetRequiredService<InboxInsertInterceptor>()
    ]);

builder.Services.AddPostgreSqlOutbox<OrderingDbContext>();
builder.Services.AddPostgreSqlInbox<OrderingDbContext>();

builder.Services.AddMassTransitEventBusWithInbox(
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

builder.Services.AddApplicationHandlers(typeof(Program).Assembly);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddGlobalExceptionHandler();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddEndpoints(typeof(Program).Assembly);
builder.Services.AddDocs();

var app = builder.Build();

app.Services.ApplyDatabaseMigrations<OrderingDbContext>();
app.UseGlobalExceptionHandler();
app.MapEndpoints();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseDocs();
}

app.Run();