using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Catalog.IntegrationTests.Infrastructure;

public class CatalogApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:15-alpine")
        .WithDatabase("catalog_test_db")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Override Configuration for Postgres
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_dbContainer.GetConnectionString());
            
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Postgres:Host", connectionStringBuilder.Host },
                { "Postgres:Port", connectionStringBuilder.Port.ToString() },
                { "Postgres:Username", connectionStringBuilder.Username },
                { "Postgres:Password", connectionStringBuilder.Password },
                { "Postgres:Database", connectionStringBuilder.Database }
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Replace RabbitMQ with MassTransit In-Memory Test Harness
            services.AddMassTransitTestHarness();

            // Setup Mock Authentication
            services.Configure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
            });

            services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, _ => { });

            // Apply global authorization policy for testing
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(TestAuthHandler.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });
        });
    }
}