using Shared.Messaging;

namespace Clean.Architecture.Application.Orders.ConfirmOrder;

/// <summary>
/// Command to confirm an order and reserve inventory.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
public sealed record ConfirmOrderCommand(Guid OrderId) : ICommand;
