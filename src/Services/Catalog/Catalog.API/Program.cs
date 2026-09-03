using BuildingBlocks.Application;
using BuildingBlocks.Web;
using Catalog.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Database
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

// Add CQRS Handlers and Pipeline Behaviors (Logging, Validation) automatically using Scrutor
builder.Services.AddApplicationHandlers(typeof(Program).Assembly);

// Add Endpoints via our new extension
builder.Services.AddEndpoints(typeof(Program).Assembly);

var app = builder.Build();

// Map the endpoints automatically
app.MapEndpoints();

app.Run();