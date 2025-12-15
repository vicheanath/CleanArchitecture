using Clean.Architecture.Domain.Inventory;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;

namespace Clean.Architecture.Application.Inventory.ReserveInventory;

/// <summary>
/// Represents the reserve inventory command handler.
/// </summary>
internal sealed class ReserveInventoryCommandHandler : ICommandHandler<ReserveInventoryCommand>
{
    private readonly IInventoryItemRepository _inventoryItemRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReserveInventoryCommandHandler"/> class.
    /// </summary>
    /// <param name="inventoryItemRepository">The inventory item repository.</param>
    public ReserveInventoryCommandHandler(IInventoryItemRepository inventoryItemRepository)
    {
        _inventoryItemRepository = inventoryItemRepository;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(ReserveInventoryCommand request, CancellationToken cancellationToken)
    {
        var inventoryItemId = InventoryItemId.Create(request.InventoryItemId);

        var inventoryItem = await _inventoryItemRepository.GetByIdAsync(inventoryItemId, cancellationToken);
        if (inventoryItem is null)
        {
            return Result.Failure(InventoryErrors.NotFound);
        }

        if (request.Quantity <= 0)
        {
            return Result.Failure(InventoryErrors.InvalidReservationQuantity);
        }

        if (string.IsNullOrWhiteSpace(request.ReservationId))
        {
            return Result.Failure(new Error("Inventory.ReservationIdRequired", "Reservation ID is required"));
        }

        try
        {
            inventoryItem.ReserveStock(request.Quantity, request.ReservationId, request.ExpiresAt);
            _inventoryItemRepository.Update(inventoryItem);

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(new Error("Inventory.InvalidReservation", ex.Message));
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(InventoryErrors.InsufficientStock);
        }
    }
}
