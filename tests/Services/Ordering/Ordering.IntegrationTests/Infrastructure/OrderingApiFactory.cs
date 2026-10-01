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

namespace Ordering.IntegrationTests.Infrastructure;

public sealed class OrderingApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:15-alpine")
        .WithDatabase("ordering_test_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

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
        });

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

        builder.UseEnvironment("Development");
    }
}