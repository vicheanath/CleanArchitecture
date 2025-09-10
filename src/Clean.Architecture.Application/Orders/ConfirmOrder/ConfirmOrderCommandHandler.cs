using Clean.Architecture.Domain.Inventory;
using Clean.Architecture.Domain.Orders;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;

namespace Clean.Architecture.Application.Orders.ConfirmOrder;

/// <summary>
/// Handler for confirming an order and reserving inventory.
/// </summary>
internal sealed class ConfirmOrderCommandHandler : ICommandHandler<ConfirmOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryItemRepository _inventoryRepository;

    public ConfirmOrderCommandHandler(
        IOrderRepository orderRepository,
        IInventoryItemRepository inventoryRepository)
    {
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
    }

    public async Task<Result> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        // Get the order
        var order = await _orderRepository.GetByIdAsync(
            OrderId.Create(request.OrderId),
            cancellationToken);

        if (order == null)
        {
            return Result.Failure(new NotFoundError("Order.NotFound", $"Order with ID '{request.OrderId}' not found"));
        }

        if (order.Status != OrderStatus.Pending)
        {
            return Result.Failure(new ConflictError("Order.InvalidStatus", $"Order is not in pending status. Current status: {order.Status}"));
        }

        // Validate inventory availability before confirming
        var inventoryValidationResults = new List<string>();

        foreach (var item in order.Items)
        {
            var inventoryItem = await _inventoryRepository.GetByProductSkuAsync(
                item.ProductSku,
                cancellationToken);

            if (inventoryItem == null)
            {
                inventoryValidationResults.Add($"No inventory found for product SKU: {item.ProductSku}");
                continue;
            }

            if (inventoryItem.AvailableQuantity < item.Quantity)
            {
                inventoryValidationResults.Add(
                    $"Insufficient stock for product SKU: {item.ProductSku}. " +
                    $"Requested: {item.Quantity}, Available: {inventoryItem.AvailableQuantity}");
            }
        }

        if (inventoryValidationResults.Any())
        {
            return Result.Failure(new ValidationError(inventoryValidationResults));
        }

        // Confirm the order (this will raise the OrderConfirmed event which triggers inventory reservation)
        order.ConfirmOrder();

        // Save the updated order
        await _orderRepository.UpdateAsync(order, cancellationToken);

        return Result.Success();
    }

    /// <summary>
    /// Validation error for order confirmation.
    /// </summary>
    private sealed class ValidationError : Shared.Errors.Error
    {
        public ValidationError(IEnumerable<string> errors)
            : base("OrderConfirmation.ValidationFailed", string.Join("; ", errors))
        {
        }
    }
}
