using BuildingBlocks.Application;
using BuildingBlocks.Grpc.Interceptors;
using BuildingBlocks.Web.Endpoints;
using Catalog.API.Infrastructure.Data;
using BuildingBlocks.Persistence.PostgreSql;
using BuildingBlocks.Web.OpenApi;
using BuildingBlocks.Web.Exceptions;
using BuildingBlocks.Persistence.EntityFrameworkCore.Extensions;
using Catalog.API.Features.Grpc;

using BuildingBlocks.Grpc.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Explicitly configure Kestrel to support HTTP/1.1 on 8080 and HTTP/2 (H2C) on 8081
builder.WebHost.ConfigureGrpcPorts();

// Add Database
builder.Services.AddPostgresDbContext<CatalogDbContext>();

// Add CQRS Handlers and Pipeline Behaviors (Logging, Validation) automatically using Scrutor
builder.Services.AddApplicationHandlers(typeof(Program).Assembly);

// Add Default Distributed Cache (In-Memory) for CachingDecorator
builder.Services.AddDistributedMemoryCache();

// Add Global Exception Handler
builder.Services.AddGlobalExceptionHandler();

// Add Endpoints via our new extension
builder.Services.AddEndpoints(typeof(Program).Assembly);

// Add gRPC and Global Exception Interceptor
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<GlobalExceptionInterceptor>();
});

// Add API Versioning and OpenAPI
builder.Services.AddDocs();

var app = builder.Build();

// Automatically apply EF Core migrations
app.Services.ApplyDatabaseMigrations<CatalogDbContext>();

// Use Global Exception Handler
app.UseGlobalExceptionHandler();

// Map the endpoints automatically
app.MapEndpoints();

// Map gRPC Services
app.MapGrpcService<CatalogGrpcService>();

// Map OpenAPI endpoints and Scalar UI
if (app.Environment.IsDevelopment())
{
    app.UseDocs();
}

app.Run();