using BuildingBlocks.Application;
using BuildingBlocks.Persistence.Marten;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Exceptions;
using BuildingBlocks.Web.OpenApi;
using BuildingBlocks.Web.Security;
using Microsoft.Extensions.Options;
using Basket.API.Options;
using BuildingBlocks.Core.Options;

var builder = WebApplication.CreateBuilder(args);

// Add App Options
builder.Services.AddAppOptions(builder.Configuration, typeof(Program).Assembly);

// Add Marten Document Database
builder.Services.AddMartenDbContext();

// Register gRPC Client for Catalog API
builder.Services.AddGrpcClient<Catalog.Grpc.CatalogGrpcService.CatalogGrpcServiceClient>((provider, options) =>
{
    var grpcOptions = provider.GetRequiredService<IOptions<GrpcOptions>>().Value;
    options.Address = new Uri(grpcOptions.CatalogUrl);
});

// Add CQRS Handlers and Pipeline Behaviors (Logging, Validation) automatically using Scrutor
builder.Services.AddApplicationHandlers(typeof(Program).Assembly);

// Add Distributed Memory Cache (for CachingDecorator)
builder.Services.AddDistributedMemoryCache();

// Add Global Exception Handler
builder.Services.AddGlobalExceptionHandler();

// Add Endpoints via our new extension
builder.Services.AddEndpoints(typeof(Program).Assembly);

// Add API Versioning and OpenAPI
builder.Services.AddDocs();

// Add Authentication and Current User
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCurrentUser();

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