using System.Reflection;
using BuildingBlocks.Application.Behaviors;
using BuildingBlocks.Application.DomainEvents;
using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.DomainEvents;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationHandlers(this IServiceCollection services, Assembly assembly)
    {
        // Add DomainEventsDispatcher
        services.AddScoped<IDomainEventsDispatcher, DomainEventsDispatcher>();

        // Add FluentValidation validators
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        // Add CQRS Handlers using Scrutor
        services.Scan(selector => selector
            .FromAssemblies(assembly)
            .AddClasses(filter => filter.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime()

            .AddClasses(filter => filter.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime()

            .AddClasses(filter => filter.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );

        // Decorate Handlers with Logging and Validation pipelines
        services.TryDecorate(typeof(ICommandHandler<>), typeof(LoggingDecorator.CommandBaseHandler<>));
        services.TryDecorate(typeof(ICommandHandler<>), typeof(ValidationDecorator.CommandBaseHandler<>));

        services.TryDecorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.CommandHandler<,>));
        services.TryDecorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator.CommandHandler<,>));

        services.TryDecorate(typeof(IQueryHandler<,>), typeof(LoggingDecorator.QueryHandler<,>));

        // Decorate Handlers with Caching pipeline
        services.TryDecorate(typeof(ICommandHandler<>), typeof(CachingDecorator.CommandBaseHandler<>));
        services.TryDecorate(typeof(ICommandHandler<,>), typeof(CachingDecorator.CommandHandler<,>));
        services.TryDecorate(typeof(IQueryHandler<,>), typeof(CachingDecorator.QueryHandler<,>));

        return services;
    }
}