using Shared.Results;

namespace Shared.Messaging;

/// <summary>
/// Represents the command interface.
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Represents the command interface.
/// </summary>
/// <typeparam name="TResponse">The command response type.</typeparam>
public interface ICommand<TResponse>
{
}
