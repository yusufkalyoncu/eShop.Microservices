using System.Reflection;
using Gateway.API.Extensions;
using BuildingBlocks.Core.Options;
using BuildingBlocks.Web.Security;

var builder = WebApplication.CreateBuilder(args);

// Add Options automatically from this assembly
builder.Services.AddAppOptions(builder.Configuration, Assembly.GetExecutingAssembly());

// Add YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add JWT Bearer Authentication (Keycloak) via centralized extension
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Add YARP Endpoints
app.MapReverseProxy();

// Add Scalar Aggregated Docs
app.UseGatewayDocs(new Dictionary<string, string>
{
    { "catalog", "/catalog-api/openapi/v1.json" },
    { "basket", "/basket-api/openapi/v1.json" },
    { "identity", "/identity-api/openapi/v1.json" }
});

app.Run();