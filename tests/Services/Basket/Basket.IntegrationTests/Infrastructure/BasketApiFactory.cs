using Catalog.Grpc;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace Basket.IntegrationTests.Infrastructure;

public sealed class BasketApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:15-alpine")
        .WithDatabase("basket_test_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
    
    // Public mock so tests can configure it
    public CatalogGrpcService.CatalogGrpcServiceClient CatalogGrpcClientMock { get; } = Substitute.For<CatalogGrpcService.CatalogGrpcServiceClient>();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync().AsTask();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Replace dependencies
        builder.ConfigureTestServices(services =>
        {
            // Setup Mock Authentication
            services.Configure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
            });

            services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, _ => { });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(TestAuthHandler.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });

            // Mock MassTransit RabbitMQ
            services.AddMassTransitTestHarness();

            // Mock Catalog gRPC Client
            services.RemoveAll<CatalogGrpcService.CatalogGrpcServiceClient>();
            services.AddSingleton(CatalogGrpcClientMock);
        });

        // Override Configuration for Postgres
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_dbContainer.GetConnectionString());
            
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Marten:Host", connectionStringBuilder.Host },
                { "Marten:Port", connectionStringBuilder.Port.ToString() },
                { "Marten:Username", connectionStringBuilder.Username },
                { "Marten:Password", connectionStringBuilder.Password },
                { "Marten:Database", connectionStringBuilder.Database }
            });
        });

        builder.UseEnvironment("Development");
    }
}