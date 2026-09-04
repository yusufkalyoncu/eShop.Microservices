using BuildingBlocks.Core.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        Func<IServiceProvider, IEnumerable<ISaveChangesInterceptor>>? interceptors = null)
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


        services.AddDbContext<TDbContext>((sp, builder) =>
        {
            var options = sp.GetRequiredService<IOptions<PostgresDbContextOptions>>().Value;

            builder.UseNpgsql(options.ConnectionString, npgsql =>
                npgsql.EnableRetryOnFailure(options.MaxRetryCount, options.MaxRetryDelay, null));

            if (options.UseSnakeCaseNamingConvention)
                builder.UseSnakeCaseNamingConvention();

            if (interceptors is not null)
                builder.AddInterceptors(interceptors(sp));
        });

        return services;
    }
}