# CQRS Implementation Without MediatR

This solution implements the CQRS (Command Query Responsibility Segregation) pattern without using MediatR. It provides a clean, lightweight alternative that gives you full control over command and query handling.

## Overview

The implementation includes:

- **Commands and Queries**: Simple interfaces for defining commands and queries
- **Handlers**: Interfaces for implementing command and query handlers
- **Domain Events**: Support for domain events and their handlers
- **Dispatcher**: Central dispatcher to route commands, queries, and domain events to their handlers
- **Validation**: Pipeline behavior for FluentValidation integration

## Core Components

### Interfaces

- `ICommand` / `ICommand<TResponse>`: Base interfaces for commands
- `IQuery<TResponse>`: Base interface for queries
- `ICommandHandler<TCommand>` / `ICommandHandler<TCommand, TResponse>`: Interfaces for command handlers
- `IQueryHandler<TQuery, TResponse>`: Interface for query handlers
- `IDomainEventHandler<TEvent>`: Interface for domain event handlers
- `IDispatcher`: Central dispatcher interface
- `IDomainEventPublisher`: Interface for publishing domain events

### Implementation Classes

- `Dispatcher`: Central dispatcher implementation
- `DomainEventPublisher`: Domain event publisher implementation
- `ValidationPipelineBehavior`: Validation pipeline for commands and queries

## Usage

### 1. Register Services

In your `Program.cs` or `Startup.cs`:

```csharp
using Shared.Extensions;

// Register CQRS services and scan assemblies for handlers
builder.Services.AddCqrs(
    typeof(ApplicationAssemblyMarker).Assembly, // Application layer
    typeof(DomainAssemblyMarker).Assembly       // Domain layer
);
```

### 2. Define Commands

```csharp
using Shared.Messaging;

public record CreateUserCommand(string Email, string FirstName, string LastName) : ICommand<Guid>;
```

### 3. Implement Command Handlers

```csharp
using Shared.Messaging;
using Shared.Results;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Implementation logic
        var userId = Guid.NewGuid();
        // ... create user logic
        return Result.Success(userId);
    }
}
```

### 4. Define Queries

```csharp
using Shared.Messaging;

public record GetUserByIdQuery(Guid UserId) : IQuery<UserDto>;
```

### 5. Implement Query Handlers

```csharp
using Shared.Messaging;
using Shared.Results;

public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    public async Task<Result<UserDto>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        // Implementation logic
        var user = await GetUserFromDatabase(query.UserId);
        return Result.Success(user);
    }
}
```

### 6. Define Domain Events

```csharp
using Shared.Primitives;

public record UserCreatedEvent(Guid UserId, string Email) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
```

### 7. Implement Domain Event Handlers

```csharp
using Shared.Messaging;

public class UserCreatedEventHandler : IDomainEventHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        // Handle the domain event (e.g., send welcome email)
    }
}
```

### 8. Use the Dispatcher

In your controllers or services:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public UsersController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserCommand command)
    {
        var result = await _dispatcher.CommandAsync(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _dispatcher.QueryAsync<GetUserByIdQuery, UserDto>(query);
        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }
}
```

### 9. Publishing Domain Events

```csharp
public class UserService
{
    private readonly IDomainEventPublisher _eventPublisher;

    public UserService(IDomainEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public async Task CreateUserAsync(CreateUserCommand command)
    {
        // Create user logic...
        var userId = Guid.NewGuid();

        // Publish domain event
        var userCreatedEvent = new UserCreatedEvent(userId, command.Email);
        await _eventPublisher.PublishAsync(userCreatedEvent);
    }
}
```

## Benefits

- **No External Dependencies**: No need for MediatR or other external libraries
- **Full Control**: Complete control over the implementation and behavior
- **Performance**: Direct method calls without reflection overhead
- **Simplicity**: Clean, simple interfaces that are easy to understand and test
- **Flexibility**: Easy to extend and customize for specific needs

## Features

- ✅ Command handling with and without return values
- ✅ Query handling
- ✅ Domain event publishing and handling
- ✅ FluentValidation integration
- ✅ Automatic handler registration
- ✅ Cancellation token support
- ✅ Result pattern integration
