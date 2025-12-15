using Clean.Architecture.Domain.Inventory;
using Clean.Architecture.Domain.Inventory.Events;
using Shared.Primitives;

namespace Clean.Architecture.Domain.UnitTests.Inventory;

public class InventoryItemTests
{
    [Fact]
    public void Create_WithValidParameters_ReturnsInventoryItem()
    {
        // Arrange
        var productSku = "SKU-001";
        var initialQuantity = 100;
        var minimumStockLevel = 10;

        // Act
        var inventoryItem = InventoryItem.Create(productSku, initialQuantity, minimumStockLevel);

        // Assert
        Assert.NotNull(inventoryItem);
        Assert.Equal(productSku, inventoryItem.ProductSku);
        Assert.Equal(initialQuantity, inventoryItem.Quantity);
        Assert.Equal(minimumStockLevel, inventoryItem.MinimumStockLevel);
        Assert.False(inventoryItem.IsOutOfStock);
        Assert.False(inventoryItem.IsBelowMinimumStock);
        Assert.Equal(0, inventoryItem.ReservedQuantity);
        Assert.Equal(initialQuantity, inventoryItem.AvailableQuantity);
        Assert.NotNull(inventoryItem.Id);
        Assert.NotEqual(default(DateTime), inventoryItem.CreatedOnUtc);
        Assert.Null(inventoryItem.ModifiedOnUtc);
    }

    [Fact]
    public void Create_WithEmptySku_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => InventoryItem.Create("", 100, 10));
        Assert.Throws<ArgumentException>(() => InventoryItem.Create("   ", 100, 10));
        Assert.Throws<ArgumentException>(() => InventoryItem.Create(null!, 100, 10));
    }

    [Fact]
    public void Create_WithNegativeInitialQuantity_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => InventoryItem.Create("SKU-001", -1, 10));
    }

    [Fact]
    public void Create_WithNegativeMinimumStockLevel_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => InventoryItem.Create("SKU-001", 100, -1));
    }

    [Fact]
    public void Create_RaisesInventoryItemCreatedDomainEvent()
    {
        // Arrange
        var productSku = "SKU-001";
        var initialQuantity = 100;
        var minimumStockLevel = 10;

        // Act
        var inventoryItem = InventoryItem.Create(productSku, initialQuantity, minimumStockLevel);

        // Assert
        var domainEvents = inventoryItem.GetDomainEvents();
        Assert.Single(domainEvents);
        Assert.IsType<InventoryItemCreatedDomainEvent>(domainEvents[0]);
        var createdEvent = (InventoryItemCreatedDomainEvent)domainEvents[0];
        Assert.Equal(inventoryItem.Id, createdEvent.InventoryItemId);
        Assert.Equal(productSku, createdEvent.ProductSku);
        Assert.Equal(initialQuantity, createdEvent.InitialQuantity);
        Assert.Equal(minimumStockLevel, createdEvent.MinimumStockLevel);
    }

    [Fact]
    public void Create_WithQuantityBelowMinimum_RaisesLowStockWarningEvent()
    {
        // Arrange
        var productSku = "SKU-001";
        var initialQuantity = 5;
        var minimumStockLevel = 10;

        // Act
        var inventoryItem = InventoryItem.Create(productSku, initialQuantity, minimumStockLevel);

        // Assert
        var domainEvents = inventoryItem.GetDomainEvents();
        Assert.Equal(2, domainEvents.Count); // Created + LowStockWarning
        Assert.Contains(domainEvents, e => e is LowStockWarningDomainEvent);
    }

    [Fact]
    public void Create_WithZeroQuantity_IsOutOfStock()
    {
        // Arrange
        var productSku = "SKU-001";
        var initialQuantity = 0;
        var minimumStockLevel = 10;

        // Act
        var inventoryItem = InventoryItem.Create(productSku, initialQuantity, minimumStockLevel);

        // Assert
        Assert.True(inventoryItem.IsOutOfStock);
        // Note: OutOfStockDomainEvent is only raised when stock decreases to zero, not on initial creation
        var domainEvents = inventoryItem.GetDomainEvents();
        Assert.Contains(domainEvents, e => e is InventoryItemCreatedDomainEvent);
    }

    [Fact]
    public void IncreaseStock_WithValidQuantity_IncreasesQuantity()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        var initialQuantity = inventoryItem.Quantity;
        var quantityToAdd = 50;

        // Act
        inventoryItem.IncreaseStock(quantityToAdd, "Restock");

        // Assert
        Assert.Equal(initialQuantity + quantityToAdd, inventoryItem.Quantity);
        Assert.NotNull(inventoryItem.ModifiedOnUtc);
    }

    [Fact]
    public void IncreaseStock_RaisesInventoryStockIncreasedDomainEvent()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        var previousQuantity = inventoryItem.Quantity;
        inventoryItem.ClearDomainEvents();

        // Act
        inventoryItem.IncreaseStock(50, "Restock");

        // Assert
        var domainEvents = inventoryItem.GetDomainEvents();
        Assert.Single(domainEvents);
        var stockIncreasedEvent = Assert.IsType<InventoryStockIncreasedDomainEvent>(domainEvents[0]);
        Assert.Equal(previousQuantity, stockIncreasedEvent.PreviousQuantity);
        Assert.Equal(150, stockIncreasedEvent.NewQuantity);
        Assert.Equal(50, stockIncreasedEvent.QuantityAdded);
        Assert.Equal("Restock", stockIncreasedEvent.Reason);
    }

    [Fact]
    public void IncreaseStock_WithZeroOrNegativeQuantity_ThrowsArgumentException()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => inventoryItem.IncreaseStock(0, "Restock"));
        Assert.Throws<ArgumentException>(() => inventoryItem.IncreaseStock(-10, "Restock"));
    }

    [Fact]
    public void DecreaseStock_WithValidQuantity_DecreasesQuantity()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        var initialQuantity = inventoryItem.Quantity;
        var quantityToRemove = 30;

        // Act
        inventoryItem.DecreaseStock(quantityToRemove, "Sale");

        // Assert
        Assert.Equal(initialQuantity - quantityToRemove, inventoryItem.Quantity);
        Assert.NotNull(inventoryItem.ModifiedOnUtc);
    }

    [Fact]
    public void DecreaseStock_RaisesInventoryStockDecreasedDomainEvent()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        var previousQuantity = inventoryItem.Quantity;
        inventoryItem.ClearDomainEvents();

        // Act
        inventoryItem.DecreaseStock(30, "Sale");

        // Assert
        var domainEvents = inventoryItem.GetDomainEvents();
        Assert.Single(domainEvents);
        var stockDecreasedEvent = Assert.IsType<InventoryStockDecreasedDomainEvent>(domainEvents[0]);
        Assert.Equal(previousQuantity, stockDecreasedEvent.PreviousQuantity);
        Assert.Equal(70, stockDecreasedEvent.NewQuantity);
        Assert.Equal(30, stockDecreasedEvent.QuantityRemoved);
        Assert.Equal("Sale", stockDecreasedEvent.Reason);
    }

    [Fact]
    public void DecreaseStock_WithInsufficientStock_ThrowsInvalidOperationException()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => inventoryItem.DecreaseStock(101, "Sale"));
    }

    [Fact]
    public void DecreaseStock_BelowMinimumStock_RaisesLowStockWarningEvent()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        inventoryItem.ClearDomainEvents();

        // Act
        inventoryItem.DecreaseStock(95, "Sale");

        // Assert
        var domainEvents = inventoryItem.GetDomainEvents();
        Assert.Contains(domainEvents, e => e is LowStockWarningDomainEvent);
    }

    [Fact]
    public void DecreaseStock_ToZero_RaisesOutOfStockEvent()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        inventoryItem.ClearDomainEvents();

        // Act
        inventoryItem.DecreaseStock(100, "Sale");

        // Assert
        Assert.True(inventoryItem.IsOutOfStock);
        var domainEvents = inventoryItem.GetDomainEvents();
        Assert.Contains(domainEvents, e => e is OutOfStockDomainEvent);
    }

    [Fact]
    public void UpdateMinimumStockLevel_WithValidLevel_UpdatesMinimumStockLevel()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        var newMinimumStockLevel = 20;

        // Act
        inventoryItem.UpdateMinimumStockLevel(newMinimumStockLevel);

        // Assert
        Assert.Equal(newMinimumStockLevel, inventoryItem.MinimumStockLevel);
        Assert.NotNull(inventoryItem.ModifiedOnUtc);
    }

    [Fact]
    public void UpdateMinimumStockLevel_RaisesMinimumStockLevelUpdatedDomainEvent()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        var previousMinimumStockLevel = inventoryItem.MinimumStockLevel;
        inventoryItem.ClearDomainEvents();

        // Act
        inventoryItem.UpdateMinimumStockLevel(20);

        // Assert
        var domainEvents = inventoryItem.GetDomainEvents();
        Assert.Single(domainEvents);
        var updatedEvent = Assert.IsType<MinimumStockLevelUpdatedDomainEvent>(domainEvents[0]);
        Assert.Equal(previousMinimumStockLevel, updatedEvent.PreviousMinimumStockLevel);
        Assert.Equal(20, updatedEvent.NewMinimumStockLevel);
    }

    [Fact]
    public void UpdateMinimumStockLevel_WithNegativeValue_ThrowsArgumentException()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => inventoryItem.UpdateMinimumStockLevel(-1));
    }

    [Fact]
    public void ReserveStock_WithValidParameters_ReservesStock()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        var reservationId = "RES-001";
        var quantityToReserve = 20;

        // Act
        inventoryItem.ReserveStock(quantityToReserve, reservationId);

        // Assert
        Assert.Equal(20, inventoryItem.ReservedQuantity);
        Assert.Equal(80, inventoryItem.AvailableQuantity);
        Assert.Single(inventoryItem.Reservations);
        Assert.Equal(reservationId, inventoryItem.Reservations[0].ReservationId);
    }

    [Fact]
    public void ReserveStock_RaisesInventoryReservedDomainEvent()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        inventoryItem.ClearDomainEvents();

        // Act
        inventoryItem.ReserveStock(20, "RES-001");

        // Assert
        var domainEvents = inventoryItem.GetDomainEvents();
        Assert.Single(domainEvents);
        var reservedEvent = Assert.IsType<InventoryReservedDomainEvent>(domainEvents[0]);
        Assert.Equal(20, reservedEvent.QuantityReserved);
        Assert.Equal(80, reservedEvent.AvailableQuantity);
        Assert.Equal("RES-001", reservedEvent.ReservationId);
    }

    [Fact]
    public void ReserveStock_WithInsufficientAvailableStock_ThrowsInvalidOperationException()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        inventoryItem.ReserveStock(50, "RES-001");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => inventoryItem.ReserveStock(51, "RES-002"));
    }

    [Fact]
    public void ReserveStock_WithDuplicateReservationId_ThrowsInvalidOperationException()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        inventoryItem.ReserveStock(20, "RES-001");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => inventoryItem.ReserveStock(10, "RES-001"));
    }

    [Fact]
    public void ReserveStock_WithExpirationDate_CreatesReservationWithExpiration()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        // Act
        inventoryItem.ReserveStock(20, "RES-001", expiresAt);

        // Assert
        var reservation = inventoryItem.Reservations.First();
        Assert.Equal(expiresAt, reservation.ExpiresAt);
        Assert.False(reservation.IsExpired);
    }

    [Fact]
    public void ReleaseReservation_WithValidReservationId_ReleasesReservation()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        inventoryItem.ReserveStock(20, "RES-001");
        var reservedQuantity = inventoryItem.ReservedQuantity;

        // Act
        inventoryItem.ReleaseReservation("RES-001");

        // Assert
        Assert.Equal(0, inventoryItem.ReservedQuantity);
        Assert.Equal(100, inventoryItem.AvailableQuantity);
        Assert.Empty(inventoryItem.Reservations);
    }

    [Fact]
    public void ReleaseReservation_RaisesInventoryReservationReleasedDomainEvent()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        inventoryItem.ReserveStock(20, "RES-001");
        inventoryItem.ClearDomainEvents();

        // Act
        inventoryItem.ReleaseReservation("RES-001");

        // Assert
        var domainEvents = inventoryItem.GetDomainEvents();
        Assert.Single(domainEvents);
        var releasedEvent = Assert.IsType<InventoryReservationReleasedDomainEvent>(domainEvents[0]);
        Assert.Equal(20, releasedEvent.QuantityReleased);
        Assert.Equal(100, releasedEvent.AvailableQuantity);
        Assert.Equal("RES-001", releasedEvent.ReservationId);
    }

    [Fact]
    public void ReleaseReservation_WithNonExistentReservationId_ThrowsInvalidOperationException()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => inventoryItem.ReleaseReservation("RES-999"));
    }

    [Fact]
    public void ConfirmReservation_WithValidReservationId_RemovesReservationAndDecreasesStock()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        inventoryItem.ReserveStock(20, "RES-001");
        var initialQuantity = inventoryItem.Quantity;

        // Act
        inventoryItem.ConfirmReservation("RES-001", "Order fulfilled");

        // Assert
        Assert.Equal(0, inventoryItem.ReservedQuantity);
        Assert.Equal(initialQuantity - 20, inventoryItem.Quantity);
        Assert.Empty(inventoryItem.Reservations);
    }

    [Fact]
    public void ConfirmReservation_WithNonExistentReservationId_ThrowsInvalidOperationException()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => inventoryItem.ConfirmReservation("RES-999"));
    }

    [Fact]
    public void RemoveExpiredReservations_RemovesExpiredReservations()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        var expiredDate = DateTime.UtcNow.AddHours(-1);
        var futureDate = DateTime.UtcNow.AddHours(24);
        inventoryItem.ReserveStock(10, "RES-EXPIRED", expiredDate);
        inventoryItem.ReserveStock(20, "RES-ACTIVE", futureDate);
        inventoryItem.ClearDomainEvents();

        // Act
        inventoryItem.RemoveExpiredReservations();

        // Assert
        Assert.Single(inventoryItem.Reservations);
        Assert.Equal("RES-ACTIVE", inventoryItem.Reservations[0].ReservationId);
        Assert.Equal(20, inventoryItem.ReservedQuantity);
        var domainEvents = inventoryItem.GetDomainEvents();
        Assert.Single(domainEvents); // One release event for expired reservation
    }

    [Fact]
    public void IsOutOfStock_WhenQuantityIsZero_ReturnsTrue()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 0, 10);

        // Assert
        Assert.True(inventoryItem.IsOutOfStock);
    }

    [Fact]
    public void IsBelowMinimumStock_WhenQuantityIsBelowMinimum_ReturnsTrue()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 5, 10);

        // Assert
        Assert.True(inventoryItem.IsBelowMinimumStock);
    }

    [Fact]
    public void AvailableQuantity_WithReservations_ReturnsCorrectQuantity()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        inventoryItem.ReserveStock(30, "RES-001");
        inventoryItem.ReserveStock(20, "RES-002");

        // Assert
        Assert.Equal(50, inventoryItem.AvailableQuantity);
        Assert.Equal(50, inventoryItem.ReservedQuantity);
    }

    [Fact]
    public void AvailableQuantity_WhenReservedExceedsQuantity_ReturnsZero()
    {
        // Arrange
        var inventoryItem = InventoryItem.Create("SKU-001", 100, 10);
        inventoryItem.ReserveStock(100, "RES-001");

        // Assert
        Assert.Equal(0, inventoryItem.AvailableQuantity);
    }
}
