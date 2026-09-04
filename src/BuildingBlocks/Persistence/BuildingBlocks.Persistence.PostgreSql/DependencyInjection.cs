using BuildingBlocks.Core.Options;
using BuildingBlocks.Persistence.EntityFrameworkCore.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BuildingBlocks.Persistence.PostgreSql;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the given DbContext with PostgreSQL.
    /// </summary>
    /// <remarks>
    /// Expects a "Postgres" section in appsettings.json with the following properties:
    /// </remarks>
    /// <example>
    /// <code>
    /// {
    ///   "Postgres": {
    ///     "Username": "my_user",
    ///     "Password": "my_password",
    ///     "Host": "localhost",
    ///     "Port": 5432,
    ///     "Database": "my_db"
    ///   }
    /// }
    /// </code>
    /// </example>
    public static IServiceCollection AddPostgresDbContext<TDbContext>(
        this IServiceCollection services,
        Action<PostgresDbContextOptions>? configure = null,
        Func<IServiceProvider, IEnumerable<ISaveChangesInterceptor>>? interceptors = null,
        Action<NpgsqlDataSourceBuilder>? dataSourceBuilderAction = null,
        Action<Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null)
        where TDbContext : DbContext
    {
        services.AddOptions<PostgresDbContextOptions>()
            .BindConfiguration(PostgresDbContextOptions.SectionName)
            .ValidateFluentValidation()
            .ValidateOnStart();

        if (configure != null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PostgresDbContextOptions>>().Value;
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(options.ConnectionString);
            
            if (options.EnableDynamicJson)
            {
                dataSourceBuilder.EnableDynamicJson();
            }

            dataSourceBuilderAction?.Invoke(dataSourceBuilder);

            return dataSourceBuilder.Build();
        });

        services.TryAddScoped<DomainEventDispatcherInterceptor>();

        services.AddDbContext<TDbContext>((sp, builder) =>
        {
            var options = sp.GetRequiredService<IOptions<PostgresDbContextOptions>>().Value;
            var dataSource = sp.GetRequiredService<NpgsqlDataSource>();

            builder.UseNpgsql(dataSource, npgsql =>
            {
                npgsql.EnableRetryOnFailure(options.MaxRetryCount, options.MaxRetryDelay, null);
                npgsqlOptionsAction?.Invoke(npgsql);
            });

            if (options.UseSnakeCaseNamingConvention)
                builder.UseSnakeCaseNamingConvention();

            var domainEventDispatcher = sp.GetRequiredService<DomainEventDispatcherInterceptor>();
            var allInterceptors = new List<IInterceptor> { domainEventDispatcher };

            if (interceptors is not null)
                allInterceptors.AddRange(interceptors(sp));

            builder.AddInterceptors(allInterceptors);
        });

        return services;
    }
}