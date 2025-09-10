using Shared.Primitives;

namespace Shared.Messaging;

/// <summary>
/// Represents the domain event publisher interface.
/// </summary>
public interface IDomainEventPublisher
{
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
    Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
