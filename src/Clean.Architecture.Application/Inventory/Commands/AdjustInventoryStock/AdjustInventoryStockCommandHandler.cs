using Clean.Architecture.Domain.Inventory;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;

namespace Clean.Architecture.Application.Inventory.Commands.AdjustInventoryStock;

/// <summary>
/// Represents the adjust inventory stock command handler.
/// </summary>
internal sealed class AdjustInventoryStockCommandHandler : ICommandHandler<AdjustInventoryStockCommand>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdjustInventoryStockCommandHandler"/> class.
    /// </summary>
    /// <param name="inventoryItemRepository">The inventory item repository.</param>
    public AdjustInventoryStockCommandHandler(IInventoryItemRepository inventoryItemRepository)
    {
        _inventoryItemRepository = inventoryItemRepository;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(AdjustInventoryStockCommand request, CancellationToken cancellationToken)
    {
        var inventoryItemId = InventoryItemId.Create(request.InventoryItemId);

        var inventoryItem = await _inventoryItemRepository.GetByIdAsync(inventoryItemId, cancellationToken);
        if (inventoryItem is null)
        {
            return Result.Failure(InventoryErrors.NotFound);
        }

        if (request.QuantityChange == 0)
        {
            return Result.Success();
        }

        try
        {
            if (request.QuantityChange > 0)
            {
                inventoryItem.IncreaseStock(request.QuantityChange, request.Reason);
            }
            else
            {
                inventoryItem.DecreaseStock(Math.Abs(request.QuantityChange), request.Reason);
            }

            _inventoryItemRepository.Update(inventoryItem);

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(new Error("Inventory.InvalidOperation", ex.Message));
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(InventoryErrors.InsufficientStock);
        }
    }
}
