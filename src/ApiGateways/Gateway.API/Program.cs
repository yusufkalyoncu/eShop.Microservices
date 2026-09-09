using System.Reflection;
using Gateway.API.Extensions;
using BuildingBlocks.Core.Options;
using Gateway.API.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add Options automatically from this assembly
builder.Services.AddAppOptions(builder.Configuration, Assembly.GetExecutingAssembly());

var identityOptions = builder.Configuration.GetSection(IdentityOptions.SectionName).Get<IdentityOptions>()
    ?? throw new InvalidOperationException("Identity configuration is missing.");

// Add YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add JWT Bearer Authentication (Keycloak)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = identityOptions.Authority;
        options.Audience = identityOptions.Audience;
        options.RequireHttpsMetadata = false; // Dev environment only
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Add YARP Endpoints
app.MapReverseProxy();

// Add Scalar Aggregated Docs
app.UseGatewayDocs(new Dictionary<string, string>
{
    { "catalog", "/catalog-api/openapi/v1.json" },
    { "basket", "/basket-api/openapi/v1.json" }
});

app.Run();