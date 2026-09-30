using Identity.API.Infrastructure.Data;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.Keycloak;
using Testcontainers.PostgreSql;

namespace Identity.IntegrationTests.Infrastructure;

public sealed class IdentityApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private readonly KeycloakContainer _keycloakContainer;

    public IdentityApiFactory()
    {
        _dbContainer = new PostgreSqlBuilder("postgres:15-alpine")
            .WithDatabase("identity_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (currentDir != null && !currentDir.GetFiles("*.sln").Any())
        {
            currentDir = currentDir.Parent;
        }
        var solutionDir = currentDir?.FullName ?? "";
        var realmExportPath = Path.Combine(solutionDir, "infrastructure", "keycloak", "realm-export.json");

        if (!File.Exists(realmExportPath))
        {
            throw new FileNotFoundException($"Realm export file not found at: {realmExportPath}");
        }

        _keycloakContainer = new KeycloakBuilder("quay.io/keycloak/keycloak:24.0.1")
            .WithBindMount(realmExportPath, "/opt/keycloak/data/import/realm.json")
            .WithCommand("--import-realm")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_dbContainer.StartAsync(), _keycloakContainer.StartAsync());
    }

    public new async Task DisposeAsync()
    {
        await Task.WhenAll(_dbContainer.DisposeAsync().AsTask(), _keycloakContainer.DisposeAsync().AsTask());
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Override DbContext
            services.RemoveAll(typeof(DbContextOptions<IdentityDbContext>));
            services.AddDbContext<IdentityDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            // Mock MassTransit RabbitMQ
            services.AddMassTransitTestHarness();
        });

        builder.UseEnvironment("Development");

        var keycloakBaseUrl = _keycloakContainer.GetBaseAddress();
        // In Testcontainers.Keycloak, GetBaseAddress() usually returns "http://127.0.0.1:xxxxx/"
        // Let's ensure it has no trailing slash to match appsettings
        var keycloakUrl = keycloakBaseUrl.TrimEnd('/');
        var keycloakAuthority = $"{keycloakUrl}/realms/eShopRealm";

        builder.UseSetting("Keycloak:Url", keycloakUrl);
        builder.UseSetting("Keycloak:Authority", keycloakAuthority);
        builder.UseSetting("Identity:Authority", keycloakAuthority);
    }
}