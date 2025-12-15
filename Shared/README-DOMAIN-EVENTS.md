# Domain Events Architecture

## Overview

This document explains how domain events work in the Clean Architecture implementation with manual dependency registration. The system uses a custom domain event publisher (not MediatR) that integrates with Entity Framework Core to automatically publish events when entities are saved.

## Domain Events Flow

```
┌─────────────────┐
│ Domain Entity   │
│ (Product, Order) │
└────────┬────────┘
         │ Raises event via RaiseDomainEvent()
         ▼
┌─────────────────────────────┐
│ Entity._domainEvents        │
│ (internal collection)       │
└────────┬────────────────────┘
         │
         │ SaveChanges() / SaveChangesAsync()
         ▼
┌─────────────────────────────┐
│ PublishDomainEventsInterceptor│
│ (EF Core Interceptor)        │
└────────┬────────────────────┘
         │ Collects all events from change tracker
         │ Clears events from entities
         ▼
┌─────────────────────────────┐
│ IDomainEventPublisher        │
│ PublishAsync(events)         │
└────────┬────────────────────┘
         │
         │ For each event type:
         │ GetServices<IDomainEventHandler<TEvent>>()
         ▼
┌─────────────────────────────┐
│ All Registered Handlers      │
│ (resolved from DI container) │
└────────┬────────────────────┘
         │
         │ Task.WhenAll() - parallel execution
         ▼
┌─────────────────────────────┐
│ Handler Execution            │
│ (all handlers run concurrently)│
└─────────────────────────────┘
```

## How Domain Events Work

### 1. Event Raising

Domain entities raise events using the protected `RaiseDomainEvent()` method inherited from `Entity<TEntityId>`:

```csharp
public sealed class Product : Entity<ProductId>
{
    public static Product Create(string sku, string name, ...)
    {
        var product = new Product(...);

        // Raise domain event
        product.RaiseDomainEvent(new ProductCreatedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            product.Id.Value,
            sku,
            name));

        return product;
    }
}
```

**Key Points:**

- Events are stored in an internal `_domainEvents` list within the entity
- Events must implement `IDomainEvent` interface
- Events are raised during domain operations (Create, Update, etc.)
- Events are not published immediately - they're collected and published later

### 2. Event Collection

The `PublishDomainEventsInterceptor` (EF Core interceptor) automatically collects events:

```csharp
// Interceptor intercepts SaveChanges / SaveChangesAsync
public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(...)
{
    // Get all entities with domain events from change tracker
    var entitiesWithDomainEvents = context.ChangeTracker
        .Entries<IEntity>()
        .Where(entry => entry.Entity.GetDomainEvents().Any())
        .ToList();

    // Collect all domain events
    var domainEvents = entitiesWithDomainEvents
        .SelectMany(entry => entry.Entity.GetDomainEvents())
        .ToList();

    // Clear events from entities BEFORE publishing
    foreach (var entry in entitiesWithDomainEvents)
    {
        entry.Entity.ClearDomainEvents();
    }

    // Publish all events
    await _domainEventPublisher.PublishAsync(domainEvents, cancellationToken);
}
```

**Key Points:**

- Events are collected automatically when `SaveChanges()` or `SaveChangesAsync()` is called
- Events are cleared from entities before publishing (prevents duplicate publishing)
- All events from all modified entities are collected in one batch

### 3. Event Publishing

The `DomainEventPublisher` publishes events using the DI container:

```csharp
public async Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, ...)
{
    foreach (var domainEvent in domainEvents)
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

        // Get ALL registered handlers for this event type
        var handlers = _serviceProvider.GetServices(handlerType);

        // Execute all handlers in parallel
        foreach (var handler in handlers)
        {
            var handleMethod = handlerType.GetMethod("Handle");
            var task = (Task)handleMethod.Invoke(handler, new object[] { domainEvent, cancellationToken })!;
            tasks.Add(task);
        }
    }

    await Task.WhenAll(tasks);
}
```

**Key Points:**

- Uses `IServiceProvider.GetServices<T>()` to find ALL registered handlers
- Supports multiple handlers per event type (all will execute)
- Handlers execute in parallel using `Task.WhenAll()`
- No ordering guarantees between handlers

### 4. Handler Execution

All registered handlers for an event type execute concurrently:

```csharp
public class ProductCreatedDomainEventHandler : IDomainEventHandler<ProductCreatedDomainEvent>
{
    public async Task Handle(ProductCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Handler logic here
        // This runs in parallel with other handlers for the same event
    }
}
```

**Key Points:**

- Each handler runs independently
- Handlers execute in parallel (not sequentially)
- No guarantee of execution order
- If one handler fails, others still execute (no transaction rollback)

## Manual Registration Requirements

### Command Handlers

Register command handlers in `Program.cs`:

```csharp
// Command handler without response
builder.Services.AddScoped<ICommandHandler<DeleteProductCommand>, DeleteProductCommandHandler>();

// Command handler with response
builder.Services.AddScoped<ICommandHandler<CreateProductCommand, CreateProductResult>, CreateProductCommandHandler>();
```

### Query Handlers

Register query handlers in `Program.cs`:

```csharp
builder.Services.AddScoped<IQueryHandler<GetAllProductsQuery, IReadOnlyList<ProductDto>>, GetAllProductsQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetProductByIdQuery, ProductDto?>, GetProductByIdQueryHandler>();
```

### Domain Event Handlers

Register domain event handlers in `Program.cs`:

```csharp
// Single handler per event
builder.Services.AddScoped<IDomainEventHandler<ProductCreatedDomainEvent>, ProductCreatedDomainEventHandler>();

// Multiple handlers for the same event (both will execute)
builder.Services.AddScoped<IDomainEventHandler<ProductCreatedDomainEvent>, ProductCreatedDomainEventHandler1>();
builder.Services.AddScoped<IDomainEventHandler<ProductCreatedDomainEvent>, ProductCreatedDomainEventHandler2>();
```

**Important:** When you have multiple handlers for the same event type (e.g., in different namespaces), register each one separately. All registered handlers will execute when the event is published.

### Example: Multiple Handlers for Same Event

```csharp
// Handler in Products.EventHandlers namespace
builder.Services.AddScoped<IDomainEventHandler<ProductCreatedDomainEvent>,
    Clean.Architecture.Application.Products.EventHandlers.ProductCreatedDomainEventHandler>();

// Handler in EventHandlers.Products namespace
builder.Services.AddScoped<IDomainEventHandler<ProductCreatedDomainEvent>,
    Clean.Architecture.Application.EventHandlers.Products.ProductCreatedDomainEventHandler>();
```

Both handlers will execute when `ProductCreatedDomainEvent` is published.

## Domain Event Publisher Registration

The domain event publisher must be registered in the DI container:

```csharp
// In Program.cs or DependencyInjection.cs
builder.Services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();
```

This is typically done in the persistence layer's `AddPersistence()` extension method.

## EF Core Interceptor Registration

The interceptor must be registered and added to the DbContext:

```csharp
// Register interceptor
services.AddScoped<PublishDomainEventsInterceptor>();

// Add to DbContext
services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var interceptor = serviceProvider.GetRequiredService<PublishDomainEventsInterceptor>();
    options.UseInMemoryDatabase("CleanArchitectureDb")
           .AddInterceptors(interceptor);
});
```

## Key Architecture Decisions

### Why Not MediatR?

- **Explicit Control**: Manual registration makes dependencies explicit and visible
- **No Magic**: No reflection-based automatic discovery - you see exactly what's registered
- **Lightweight**: Custom implementation is simpler and has fewer dependencies
- **Flexibility**: Easy to customize behavior (e.g., error handling, logging)

### Why Manual Registration?

- **Explicit Dependencies**: All handlers are visible in `Program.cs`
- **Compile-Time Safety**: Missing registrations cause compile errors when handlers are injected
- **Easy to Review**: Code reviews can easily see what's registered
- **No Reflection Overhead**: No runtime scanning of assemblies

### Why Multiple Handlers Per Event?

- **Separation of Concerns**: Different handlers can handle different aspects of an event
- **Extensibility**: Easy to add new handlers without modifying existing ones
- **Parallel Execution**: All handlers run concurrently for better performance

## Best Practices

1. **Raise Events in Domain Methods**: Raise events in domain entity methods, not in application handlers
2. **Idempotent Handlers**: Design handlers to be idempotent when possible
3. **Error Handling**: Consider error handling strategy for handler failures
4. **Logging**: Log important events and handler executions
5. **Testing**: Test handlers independently and test event publishing flow

## Example: Complete Flow

1. **Domain Entity Raises Event**:

   ```csharp
   var product = Product.Create(sku, name, ...);
   // ProductCreatedDomainEvent is added to product._domainEvents
   ```

2. **Application Handler Saves Entity**:

   ```csharp
   await _productRepository.AddAsync(product, cancellationToken);
   await _unitOfWork.SaveChangesAsync(cancellationToken);
   // SaveChanges triggers interceptor
   ```

3. **Interceptor Collects Events**:

   ```csharp
   // PublishDomainEventsInterceptor collects ProductCreatedDomainEvent
   ```

4. **Publisher Resolves Handlers**:

   ```csharp
   // DomainEventPublisher finds all IDomainEventHandler<ProductCreatedDomainEvent>
   // Resolves: ProductCreatedDomainEventHandler1, ProductCreatedDomainEventHandler2
   ```

5. **Handlers Execute**:
   ```csharp
   // Both handlers execute in parallel
   // Handler1: Creates inventory item
   // Handler2: Sends notification
   ```

## Troubleshooting

### Handlers Not Executing

1. **Check Registration**: Ensure handler is registered in `Program.cs`
2. **Check Event Type**: Ensure event type matches handler's generic parameter
3. **Check SaveChanges**: Events only publish when `SaveChanges()` is called
4. **Check Interceptor**: Ensure interceptor is registered and added to DbContext

### Multiple Handlers Not Executing

- Ensure each handler is registered separately
- Use fully qualified type names if handlers are in different namespaces
- Check that `GetServices<T>()` is used (not `GetService<T>()`)

### Events Published Multiple Times

- Events are cleared from entities before publishing
- If you see duplicates, check for multiple `SaveChanges()` calls
- Ensure interceptor is only registered once

## Summary

- **No MediatR**: Uses custom `IDomainEventPublisher` with DI container
- **Manual Registration**: All handlers must be explicitly registered in `Program.cs`
- **Multiple Handlers**: Same event can have multiple handlers (all execute in parallel)
- **EF Core Integration**: Events automatically published on `SaveChanges`
- **Parallel Execution**: Handlers run concurrently, not sequentially
- **Explicit Control**: All dependencies are visible and explicit
