using BuildingBlocks.Application;
using BuildingBlocks.Web.Endpoints;
using Catalog.API.Infrastructure.Data;
using BuildingBlocks.Persistence.PostgreSql;
using BuildingBlocks.Web.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add Database
builder.Services.AddPostgresDbContext<CatalogDbContext>();

// Add CQRS Handlers and Pipeline Behaviors (Logging, Validation) automatically using Scrutor
builder.Services.AddApplicationHandlers(typeof(Program).Assembly);

// Add Endpoints via our new extension
builder.Services.AddEndpoints(typeof(Program).Assembly);

// Add API Versioning and OpenAPI
builder.Services.AddDocs();

var app = builder.Build();

// Map the endpoints automatically
app.MapEndpoints();

// Map OpenAPI endpoints and Scalar UI
if (app.Environment.IsDevelopment())
{
    app.UseDocs();
}

app.Run();