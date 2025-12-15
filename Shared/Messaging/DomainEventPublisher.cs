using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Primitives;
using System.Reflection;

namespace Shared.Messaging;

/// <summary>
/// Implementation of domain event publisher that resolves and executes domain event handlers.
/// </summary>
public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventPublisher>? _logger;

    public DomainEventPublisher(IServiceProvider serviceProvider, ILogger<DomainEventPublisher>? logger = null)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        if (domainEvent == null)
        {
            throw new ArgumentNullException(nameof(domainEvent));
        }

        var handlers = _serviceProvider.GetServices<IDomainEventHandler<TEvent>>().ToList();

        if (handlers.Count == 0)
        {
            _logger?.LogDebug("No handlers found for domain event {EventType}", typeof(TEvent).Name);
            return;
        }

        var tasks = handlers.Select(handler => ExecuteHandlerSafely(handler, domainEvent, cancellationToken));

        await Task.WhenAll(tasks);
    }

    public async Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        if (domainEvents == null)
        {
            throw new ArgumentNullException(nameof(domainEvents));
        }

        var tasks = new List<Task>();

        foreach (var domainEvent in domainEvents)
        {
            if (domainEvent == null)
            {
                _logger?.LogWarning("Null domain event encountered, skipping");
                continue;
            }

            var eventType = domainEvent.GetType();
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
            var handlers = _serviceProvider.GetServices(handlerType).ToList();

            if (handlers.Count == 0)
            {
                _logger?.LogDebug("No handlers found for domain event {EventType}", eventType.Name);
                continue;
            }

            foreach (var handler in handlers)
            {
                if (handler == null)
                {
                    _logger?.LogWarning("Null handler encountered for event {EventType}, skipping", eventType.Name);
                    continue;
                }

                var handleMethod = handlerType.GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance);
                if (handleMethod == null)
                {
                    _logger?.LogError("Handle method not found on handler type {HandlerType}", handlerType.Name);
                    continue;
                }

                try
                {
                    var task = (Task?)handleMethod.Invoke(handler, new object[] { domainEvent, cancellationToken });
                    if (task != null)
                    {
                        tasks.Add(ExecuteReflectionHandlerSafely(task, eventType, handler.GetType()));
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to invoke handler {HandlerType} for event {EventType}",
                        handler.GetType().Name, eventType.Name);
                    // Continue with other handlers even if one fails to invoke
                }
            }
        }

        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
        }
    }

    private async Task ExecuteHandlerSafely<TEvent>(IDomainEventHandler<TEvent> handler, TEvent domainEvent, CancellationToken cancellationToken)
        where TEvent : IDomainEvent
    {
        try
        {
            await handler.Handle(domainEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Handler {HandlerType} failed while handling event {EventType}",
                handler.GetType().Name, typeof(TEvent).Name);
            // Re-throw to maintain existing behavior - caller can handle if needed
            throw;
        }
    }

    private async Task ExecuteReflectionHandlerSafely(Task task, Type eventType, Type handlerType)
    {
        try
        {
            await task;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Handler {HandlerType} failed while handling event {EventType}",
                handlerType.Name, eventType.Name);
            // Re-throw to maintain existing behavior
            throw;
        }
    }
}
