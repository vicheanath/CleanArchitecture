using Clean.Architecture.Domain.Inventory.Events;
using Shared.Primitives;

namespace Clean.Architecture.Domain.Inventory;

/// <summary>
/// Represents an inventory item entity.
/// </summary>
public sealed class InventoryItem : Entity<InventoryItemId>, IAuditable
{
    private readonly List<InventoryReservation> _reservations = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryItem"/> class.
    /// </summary>
    /// <param name="id">The inventory item identifier.</param>
    /// <param name="productSku">The product SKU.</param>
    /// <param name="quantity">The initial quantity.</param>
    /// <param name="minimumStockLevel">The minimum stock level.</param>
    private InventoryItem(InventoryItemId id, string productSku, int quantity, int minimumStockLevel)
        : base(id)
    {
        ProductSku = productSku;
        Quantity = quantity;
        MinimumStockLevel = minimumStockLevel;
        CreatedOnUtc = DateTime.UtcNow;
        ModifiedOnUtc = null;

        RaiseDomainEvent(new InventoryItemCreatedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            ProductSku,
            quantity,
            minimumStockLevel));

        CheckForLowStock();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryItem"/> class.
    /// </summary>
    /// <remarks>
    /// Required for EF Core.
    /// </remarks>
    private InventoryItem()
    {
    }

    /// <summary>
    /// Gets the product SKU.
    /// </summary>
    public string ProductSku { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the current quantity in stock.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Gets the minimum stock level that triggers low stock warnings.
    /// </summary>
    public int MinimumStockLevel { get; private set; }

    /// <summary>
    /// Gets the total reserved quantity.
    /// </summary>
    public int ReservedQuantity => _reservations.Where(r => !r.IsExpired).Sum(r => r.Quantity);

    /// <summary>
    /// Gets the available quantity (not reserved).
    /// </summary>
    public int AvailableQuantity => Math.Max(0, Quantity - ReservedQuantity);

    /// <summary>
    /// Gets a value indicating whether the item is out of stock.
    /// </summary>
    public bool IsOutOfStock => Quantity <= 0;

    /// <summary>
    /// Gets a value indicating whether the item is below minimum stock level.
    /// </summary>
    public bool IsBelowMinimumStock => Quantity <= MinimumStockLevel;

    /// <summary>
    /// Gets the inventory reservations.
    /// </summary>
    public IReadOnlyList<InventoryReservation> Reservations => _reservations.AsReadOnly();

    /// <inheritdoc />
    public DateTime CreatedOnUtc { get; private set; }

    /// <inheritdoc />
    public DateTime? ModifiedOnUtc { get; private set; }

    /// <summary>
    /// Creates a new inventory item.
    /// </summary>
    /// <param name="productSku">The product SKU.</param>
    /// <param name="initialQuantity">The initial quantity.</param>
    /// <param name="minimumStockLevel">The minimum stock level.</param>
    /// <returns>The newly created inventory item.</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static InventoryItem Create(string productSku, int initialQuantity, int minimumStockLevel)
    {
        if (string.IsNullOrWhiteSpace(productSku))
            throw new ArgumentException("Product SKU is required", nameof(productSku));

        if (initialQuantity < 0)
            throw new ArgumentException("Initial quantity cannot be negative", nameof(initialQuantity));

        if (minimumStockLevel < 0)
            throw new ArgumentException("Minimum stock level cannot be negative", nameof(minimumStockLevel));

        return new InventoryItem(InventoryItemId.New(), productSku, initialQuantity, minimumStockLevel);
    }

    /// <summary>
    /// Increases the inventory quantity.
    /// </summary>
    /// <param name="quantity">The quantity to add.</param>
    /// <param name="reason">The reason for the increase.</param>
    /// <exception cref="ArgumentException">Thrown when quantity is invalid.</exception>
    public void IncreaseStock(int quantity, string? reason = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity to add must be greater than zero", nameof(quantity));

        var previousQuantity = Quantity;
        Quantity += quantity;
        ModifiedOnUtc = DateTime.UtcNow;

        RaiseDomainEvent(new InventoryStockIncreasedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            ProductSku,
            quantity,
            previousQuantity,
            Quantity,
            reason));
    }

    /// <summary>
    /// Decreases the inventory quantity.
    /// </summary>
    /// <param name="quantity">The quantity to remove.</param>
    /// <param name="reason">The reason for the decrease.</param>
    /// <exception cref="ArgumentException">Thrown when quantity is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when there is insufficient stock.</exception>
    public void DecreaseStock(int quantity, string? reason = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity to remove must be greater than zero", nameof(quantity));

        if (quantity > Quantity)
            throw new InvalidOperationException("Cannot remove more quantity than available in stock");

        var previousQuantity = Quantity;
        Quantity -= quantity;
        ModifiedOnUtc = DateTime.UtcNow;

        RaiseDomainEvent(new InventoryStockDecreasedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            ProductSku,
            quantity,
            previousQuantity,
            Quantity,
            reason));

        CheckForLowStock();
        CheckForOutOfStock();
    }

    /// <summary>
    /// Updates the minimum stock level.
    /// </summary>
    /// <param name="newMinimumStockLevel">The new minimum stock level.</param>
    /// <exception cref="ArgumentException">Thrown when the minimum stock level is invalid.</exception>
    public void UpdateMinimumStockLevel(int newMinimumStockLevel)
    {
        if (newMinimumStockLevel < 0)
            throw new ArgumentException("Minimum stock level cannot be negative", nameof(newMinimumStockLevel));

        var previousMinimumStockLevel = MinimumStockLevel;
        MinimumStockLevel = newMinimumStockLevel;
        ModifiedOnUtc = DateTime.UtcNow;

        RaiseDomainEvent(new MinimumStockLevelUpdatedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            ProductSku,
            previousMinimumStockLevel,
            newMinimumStockLevel));

        CheckForLowStock();
    }

    /// <summary>
    /// Reserves inventory for an order or other operation.
    /// </summary>
    /// <param name="quantity">The quantity to reserve.</param>
    /// <param name="reservationId">The reservation identifier.</param>
    /// <param name="expiresAt">The expiration date and time for the reservation.</param>
    /// <exception cref="ArgumentException">Thrown when quantity or reservation ID is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when there is insufficient available stock.</exception>
    public void ReserveStock(int quantity, string reservationId, DateTime? expiresAt = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Reservation quantity must be greater than zero", nameof(quantity));

        if (string.IsNullOrWhiteSpace(reservationId))
            throw new ArgumentException("Reservation ID is required", nameof(reservationId));

        if (_reservations.Any(r => r.ReservationId == reservationId && !r.IsExpired))
            throw new InvalidOperationException($"A reservation with ID '{reservationId}' already exists");

        if (quantity > AvailableQuantity)
            throw new InvalidOperationException("Cannot reserve more quantity than available");

        var reservation = new InventoryReservation(reservationId, quantity, DateTime.UtcNow, expiresAt);
        _reservations.Add(reservation);
        ModifiedOnUtc = DateTime.UtcNow;

        RaiseDomainEvent(new InventoryReservedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            ProductSku,
            quantity,
            AvailableQuantity,
            reservationId));
    }

    /// <summary>
    /// Releases a reservation, making the reserved quantity available again.
    /// </summary>
    /// <param name="reservationId">The reservation identifier.</param>
    /// <exception cref="ArgumentException">Thrown when reservation ID is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the reservation is not found.</exception>
    public void ReleaseReservation(string reservationId)
    {
        if (string.IsNullOrWhiteSpace(reservationId))
            throw new ArgumentException("Reservation ID is required", nameof(reservationId));

        var reservation = _reservations.FirstOrDefault(r => r.ReservationId == reservationId && !r.IsExpired);
        if (reservation is null)
            throw new InvalidOperationException($"No active reservation found with ID '{reservationId}'");

        _reservations.Remove(reservation);
        ModifiedOnUtc = DateTime.UtcNow;

        RaiseDomainEvent(new InventoryReservationReleasedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            ProductSku,
            reservation.Quantity,
            AvailableQuantity,
            reservationId));
    }

    /// <summary>
    /// Confirms a reservation by permanently removing the reserved quantity from stock.
    /// </summary>
    /// <param name="reservationId">The reservation identifier.</param>
    /// <param name="reason">The reason for confirming the reservation.</param>
    /// <exception cref="ArgumentException">Thrown when reservation ID is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the reservation is not found.</exception>
    public void ConfirmReservation(string reservationId, string? reason = null)
    {
        if (string.IsNullOrWhiteSpace(reservationId))
            throw new ArgumentException("Reservation ID is required", nameof(reservationId));

        var reservation = _reservations.FirstOrDefault(r => r.ReservationId == reservationId && !r.IsExpired);
        if (reservation is null)
            throw new InvalidOperationException($"No active reservation found with ID '{reservationId}'");

        _reservations.Remove(reservation);
        DecreaseStock(reservation.Quantity, reason ?? $"Confirmed reservation {reservationId}");
    }

    /// <summary>
    /// Removes expired reservations from the item.
    /// </summary>
    public void RemoveExpiredReservations()
    {
        var expiredReservations = _reservations.Where(r => r.IsExpired).ToList();

        foreach (var expiredReservation in expiredReservations)
        {
            _reservations.Remove(expiredReservation);

            RaiseDomainEvent(new InventoryReservationReleasedDomainEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                Id,
                ProductSku,
                expiredReservation.Quantity,
                AvailableQuantity,
                expiredReservation.ReservationId));
        }

        if (expiredReservations.Count > 0)
        {
            ModifiedOnUtc = DateTime.UtcNow;
        }
    }

    private void CheckForLowStock()
    {
        if (IsBelowMinimumStock && !IsOutOfStock)
        {
            RaiseDomainEvent(new LowStockWarningDomainEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                Id,
                ProductSku,
                Quantity,
                MinimumStockLevel));
        }
    }

    private void CheckForOutOfStock()
    {
        if (IsOutOfStock)
        {
            RaiseDomainEvent(new OutOfStockDomainEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                Id,
                ProductSku));
        }
    }
}
