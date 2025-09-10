using Shared.Primitives;

namespace Clean.Architecture.Domain.Inventory;

/// <summary>
/// Represents an inventory reservation.
/// </summary>
public sealed class InventoryReservation : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryReservation"/> class.
    /// </summary>
    /// <param name="reservationId">The reservation identifier.</param>
    /// <param name="quantity">The reserved quantity.</param>
    /// <param name="reservedAt">The reservation date and time.</param>
    /// <param name="expiresAt">The expiration date and time.</param>
    public InventoryReservation(string reservationId, int quantity, DateTime reservedAt, DateTime? expiresAt)
    {
        ReservationId = reservationId;
        Quantity = quantity;
        ReservedAt = reservedAt;
        ExpiresAt = expiresAt;
    }

    /// <summary>
    /// Gets the reservation identifier.
    /// </summary>
    public string ReservationId { get; }

    /// <summary>
    /// Gets the reserved quantity.
    /// </summary>
    public int Quantity { get; }

    /// <summary>
    /// Gets the reservation date and time.
    /// </summary>
    public DateTime ReservedAt { get; }

    /// <summary>
    /// Gets the expiration date and time.
    /// </summary>
    public DateTime? ExpiresAt { get; }

    /// <summary>
    /// Gets a value indicating whether the reservation is expired.
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

    /// <inheritdoc />
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return ReservationId;
        yield return Quantity;
        yield return ReservedAt;
        yield return ExpiresAt ?? DateTime.MinValue;
    }
}
