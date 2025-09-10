using Microsoft.Extensions.DependencyInjection;
using Shared.Messaging;
using System.Reflection;

namespace Shared.Extensions;

/// <summary>
/// Extension methods for registering CQRS services.
/// </summary>
public static class CqrsServiceCollectionExtensions
{
    /// <summary>
    /// Registers the CQRS dispatcher and handlers from the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for handlers.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddCqrs(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Register the dispatcher
        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        // Register command handlers
        foreach (var assembly in assemblies)
        {
            var commandHandlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    (i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                     i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>))))
                .ToArray();

            foreach (var handlerType in commandHandlerTypes)
            {
                var interfaces = handlerType.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                        (i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                         i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)));

                foreach (var @interface in interfaces)
                {
                    services.AddScoped(@interface, handlerType);
                }
            }

            // Register query handlers
            var queryHandlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)))
                .ToArray();

            foreach (var handlerType in queryHandlerTypes)
            {
                var interfaces = handlerType.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));

                foreach (var @interface in interfaces)
                {
                    services.AddScoped(@interface, handlerType);
                }
            }

            // Register domain event handlers
            var domainEventHandlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)))
                .ToArray();

            foreach (var handlerType in domainEventHandlerTypes)
            {
                var interfaces = handlerType.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>));

                foreach (var @interface in interfaces)
                {
                    services.AddScoped(@interface, handlerType);
                }
            }
        }

        return services;
    }
}
