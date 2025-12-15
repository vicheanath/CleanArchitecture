using Shared.Messaging;

namespace Clean.Architecture.Application.Inventory.ReserveInventory;

/// <summary>
/// Represents the reserve inventory command.
/// </summary>
/// <param name="InventoryItemId">The inventory item identifier.</param>
/// <param name="Quantity">The quantity to reserve.</param>
/// <param name="ReservationId">The reservation identifier.</param>
/// <param name="ExpiresAt">The expiration date and time for the reservation.</param>
public sealed record ReserveInventoryCommand(
    Guid InventoryItemId,
    int Quantity,
    string ReservationId,
    DateTime? ExpiresAt) : ICommand;
