using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shared.Primitives;
using Shared.Results;
using Shared.Exceptions;

namespace Shared.Messaging
{
    public class Dispatcher : IDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public Dispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
            where TQuery : IQuery<TResult>
        {
            var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
            var result = await handler.Handle(query, cancellationToken);
            if (!result.IsSuccess)
                throw new DomainException(result.Error ?? new("Query.Failed", "Query failed"));
            return result.Value;
        }

        public async Task CommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : ICommand
        {
            var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
            var result = await handler.Handle(command, cancellationToken);
            if (!result.IsSuccess)
                throw new DomainException(result.Error ?? new("Command.Failed", "Command failed"));
        }

        public async Task<TResponse> CommandAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : ICommand<TResponse>
        {
            var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResponse>>();
            var result = await handler.Handle(command, cancellationToken);
            if (!result.IsSuccess)
                throw new DomainException(result.Error ?? new("Command.Failed", "Command failed"));
            return result.Value;
        }

        // New methods that return Results directly
        public async Task<Result<TResult>> QueryResultAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
            where TQuery : IQuery<TResult>
        {
            var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
            return await handler.Handle(query, cancellationToken);
        }

        public async Task<Result> CommandResultAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : ICommand
        {
            var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
            return await handler.Handle(command, cancellationToken);
        }

        public async Task<Result<TResponse>> CommandResultAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : ICommand<TResponse>
        {
            var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResponse>>();
            return await handler.Handle(command, cancellationToken);
        }

        public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
            where TEvent : IDomainEvent
        {
            var handlers = _serviceProvider.GetServices<IDomainEventHandler<TEvent>>();
            var tasks = new List<Task>();

            foreach (var handler in handlers)
            {
                tasks.Add(handler.Handle(domainEvent, cancellationToken));
            }

            await Task.WhenAll(tasks);
        }
    }

    public interface IDispatcher
    {
        Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IQuery<TResult>;
        Task CommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand;
        Task<TResponse> CommandAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand<TResponse>;
        Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;

        // New methods that return Results directly
        Task<Result<TResult>> QueryResultAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IQuery<TResult>;
        Task<Result> CommandResultAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand;
        Task<Result<TResponse>> CommandResultAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand<TResponse>;
    }
}
