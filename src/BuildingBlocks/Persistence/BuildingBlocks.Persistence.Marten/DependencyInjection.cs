using BuildingBlocks.Core.Options;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Weasel.Core;

namespace BuildingBlocks.Persistence.Marten;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the Marten Document Store.
    /// </summary>
    /// <remarks>
    /// Expects a "Marten" section in appsettings.json with the following properties:
    /// </remarks>
    /// <example>
    /// <code>
    /// {
    ///   "Marten": {
    ///     "Username": "postgres",
    ///     "Password": "postgres_password",
    ///     "Host": "localhost",
    ///     "Port": 5432,
    ///     "Database": "my_db",
    ///     "SchemaName": "public"
    ///   }
    /// }
    /// </code>
    /// </example>
    public static IServiceCollection AddMartenDbContext(
        this IServiceCollection services,
        Action<MartenOptions>? configure = null)
    {
        services.AddOptions<MartenOptions>()
            .BindConfiguration(MartenOptions.SectionName)
            .ValidateFluentValidation()
            .ValidateOnStart();

        if (configure != null)
        {
            services.Configure(configure);
        }

        services.ConfigureMarten((sp, opts) =>
        {
            var options = sp.GetRequiredService<IOptions<MartenOptions>>().Value;
            
            opts.Connection(options.ConnectionString);
            opts.DatabaseSchemaName = options.SchemaName;
            
            // Use System.Text.Json for serialization so [JsonInclude] works
            opts.UseSystemTextJsonForSerialization();

            // Automatically create schema in development
            opts.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
        });

        services.AddMarten().UseLightweightSessions();

        return services;
    }
}