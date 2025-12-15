using Clean.Architecture.Domain.Inventory;

namespace Clean.Architecture.Domain.UnitTests.Inventory;

public class InventoryReservationTests
{
    [Fact]
    public void Create_WithValidParameters_CreatesReservation()
    {
        // Arrange
        var reservationId = "RES-001";
        var quantity = 10;
        var reservedAt = DateTime.UtcNow;
        var expiresAt = DateTime.UtcNow.AddHours(24);

        // Act
        var reservation = new InventoryReservation(reservationId, quantity, reservedAt, expiresAt);

        // Assert
        Assert.Equal(reservationId, reservation.ReservationId);
        Assert.Equal(quantity, reservation.Quantity);
        Assert.Equal(reservedAt, reservation.ReservedAt);
        Assert.Equal(expiresAt, reservation.ExpiresAt);
    }

    [Fact]
    public void IsExpired_WhenExpirationDateIsInPast_ReturnsTrue()
    {
        // Arrange
        var reservation = new InventoryReservation(
            "RES-001",
            10,
            DateTime.UtcNow.AddHours(-2),
            DateTime.UtcNow.AddHours(-1));

        // Assert
        Assert.True(reservation.IsExpired);
    }

    [Fact]
    public void IsExpired_WhenExpirationDateIsInFuture_ReturnsFalse()
    {
        // Arrange
        var reservation = new InventoryReservation(
            "RES-001",
            10,
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow.AddHours(1));

        // Assert
        Assert.False(reservation.IsExpired);
    }

    [Fact]
    public void IsExpired_WhenNoExpirationDate_ReturnsFalse()
    {
        // Arrange
        var reservation = new InventoryReservation(
            "RES-001",
            10,
            DateTime.UtcNow,
            null);

        // Assert
        Assert.False(reservation.IsExpired);
    }

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var reservationId = "RES-001";
        var quantity = 10;
        var reservedAt = DateTime.UtcNow;
        var expiresAt = DateTime.UtcNow.AddHours(24);

        var reservation1 = new InventoryReservation(reservationId, quantity, reservedAt, expiresAt);
        var reservation2 = new InventoryReservation(reservationId, quantity, reservedAt, expiresAt);

        // Assert
        Assert.Equal(reservation1, reservation2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var reservedAt = DateTime.UtcNow;
        var expiresAt = DateTime.UtcNow.AddHours(24);

        var reservation1 = new InventoryReservation("RES-001", 10, reservedAt, expiresAt);
        var reservation2 = new InventoryReservation("RES-002", 10, reservedAt, expiresAt);

        // Assert
        Assert.NotEqual(reservation1, reservation2);
    }
}
