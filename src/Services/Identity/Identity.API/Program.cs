using BuildingBlocks.Application;
using BuildingBlocks.Core.Options;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Exceptions;
using BuildingBlocks.Web.OpenApi;
using BuildingBlocks.Web.Security;
using Keycloak.Net;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

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