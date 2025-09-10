using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Errors;
using Shared.Infrastructure;
using Shared.Messaging;
using Shared.Results;

namespace Shared.Behaviors;

public sealed class ValidationPipelineBehavior
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationPipelineBehavior(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task<Result> ValidateAndExecuteCommand<TCommand>(TCommand command, Func<TCommand, Task<Result>> handler, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        var validators = _serviceProvider.GetServices<IValidator<TCommand>>();

        if (validators.Any())
        {
            Error[] errors = Validate(command, validators);
            if (errors.Length != 0)
            {
                return ValidationResult.WithErrors(errors);
            }
        }

        return await handler(command);
    }

    public async Task<Result<TResponse>> ValidateAndExecuteCommand<TCommand, TResponse>(TCommand command, Func<TCommand, Task<Result<TResponse>>> handler, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResponse>
    {
        var validators = _serviceProvider.GetServices<IValidator<TCommand>>();

        if (validators.Any())
        {
            Error[] errors = Validate(command, validators);
            if (errors.Length != 0)
            {
                return ValidationResult<TResponse>.WithErrors(errors);
            }
        }

        return await handler(command);
    }

    public async Task<Result<TResponse>> ValidateAndExecuteQuery<TQuery, TResponse>(TQuery query, Func<TQuery, Task<Result<TResponse>>> handler, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>
    {
        var validators = _serviceProvider.GetServices<IValidator<TQuery>>();

        if (validators.Any())
        {
            Error[] errors = Validate(query, validators);
            if (errors.Length != 0)
            {
                return ValidationResult<TResponse>.WithErrors(errors);
            }
        }

        return await handler(query);
    }

    private Error[] Validate<T>(T request, IEnumerable<IValidator<T>> validators) =>
        validators.Select(validator => validator.Validate(request))
            .SelectMany(validationResult => validationResult.Errors)
            .Where(validationFailure => validationFailure is not null)
            .Select(validationFailure => new Error(validationFailure.ErrorCode, validationFailure.ErrorMessage))
            .Distinct()
            .ToArray();
}
