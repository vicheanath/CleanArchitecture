using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Messaging;
using Shared.Primitives;

namespace Clean.Architecture.Persistence.Interceptors;

/// <summary>
/// Interceptor that publishes domain events when SaveChanges is called
/// </summary>
public sealed class PublishDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IDomainEventPublisher _domainEventPublisher;

    public PublishDomainEventsInterceptor(IDomainEventPublisher domainEventPublisher)
    {
        _domainEventPublisher = domainEventPublisher;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        PublishDomainEvents(eventData.Context).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await PublishDomainEvents(eventData.Context, cancellationToken);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task PublishDomainEvents(DbContext? context, CancellationToken cancellationToken = default)
    {
        if (context is null)
        {
            return;
        }

        // Get all entities with domain events
        var entitiesWithDomainEvents = context.ChangeTracker
            .Entries<IEntity>()
            .Where(entry => entry.Entity.GetDomainEvents().Any())
            .ToList();

        // Collect all domain events
        var domainEvents = entitiesWithDomainEvents
            .SelectMany(entry => entry.Entity.GetDomainEvents())
            .ToList();

        // Clear domain events from entities before publishing
        foreach (var entry in entitiesWithDomainEvents)
        {
            entry.Entity.ClearDomainEvents();
        }

        // Publish all domain events
        if (domainEvents.Any())
        {
            await _domainEventPublisher.PublishAsync(domainEvents, cancellationToken);
        }
    }
}
