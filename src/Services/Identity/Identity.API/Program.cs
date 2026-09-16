using BuildingBlocks.Application;
using BuildingBlocks.Observability;
using BuildingBlocks.Core.Options;
using BuildingBlocks.Messaging.MassTransit;
using BuildingBlocks.Messaging.MassTransit.Options;
using BuildingBlocks.Outbox.PostgreSql;
using BuildingBlocks.Persistence.EntityFrameworkCore.Extensions;
using BuildingBlocks.Persistence.PostgreSql;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Exceptions;
using BuildingBlocks.Web.OpenApi;
using BuildingBlocks.Web.Security;
using Identity.API.Infrastructure.Data;
using Identity.API.Options;
using Keycloak.Net;
using MassTransit;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability("Identity.API");

// Add Options with Validation
builder.Services.AddAppOptions(builder.Configuration, typeof(Program).Assembly);

// Add CQRS Handlers and Pipeline Behaviors (Logging, Validation) automatically using Scrutor
builder.Services.AddApplicationHandlers(typeof(Program).Assembly);

// Add Global Exception Handler
builder.Services.AddGlobalExceptionHandler();

// Add Endpoints via our new extension
builder.Services.AddEndpoints(typeof(Program).Assembly);

// Add API Versioning and OpenAPI
builder.Services.AddDocs();

// Add Authentication using centralized extension
builder.Services.AddJwtAuthentication(builder.Configuration);

// HttpClient for Keycloak Token Endpoint
builder.Services.AddHttpClient("Keycloak", (provider, client) =>
{
    var keycloakOptions = provider.GetRequiredService<IOptions<Identity.API.Options.KeycloakOptions>>().Value;
    client.BaseAddress = new Uri(keycloakOptions.Url);
});

// Configure Keycloak Admin Client
builder.Services.AddSingleton(provider =>
{
    var keycloakOptions = provider.GetRequiredService<IOptions<Identity.API.Options.KeycloakOptions>>().Value;
    return new KeycloakClient(
        keycloakOptions.Url,
        keycloakOptions.ClientSecret,
        new Keycloak.Net.KeycloakOptions(adminClientId: keycloakOptions.ClientId, authenticationRealm: keycloakOptions.Realm)
    );
});

// Add PostgreSQL DbContext (for Outbox only)
builder.Services.AddPostgresDbContext<IdentityDbContext>(interceptors: sp =>
    [sp.GetRequiredService<BuildingBlocks.Outbox.EntityFrameworkCore.OutboxInsertInterceptor>()]);

// Add Outbox Pattern
builder.Services.AddPostgreSqlOutbox<IdentityDbContext>();

// Add MassTransit with RabbitMQ (publisher only — no consumers in Identity)
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

var app = builder.Build();

// Apply EF Core migrations on startup
app.Services.ApplyDatabaseMigrations<IdentityDbContext>();

// Use Global Exception Handler
app.UseGlobalExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

// Map the endpoints automatically
app.MapEndpoints();

// Map OpenAPI endpoints and Scalar UI
if (app.Environment.IsDevelopment())
{
    app.UseDocs();
}

app.Run();