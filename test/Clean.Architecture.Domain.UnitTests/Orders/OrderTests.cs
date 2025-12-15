using Clean.Architecture.Domain.Orders;

namespace Clean.Architecture.Domain.UnitTests.Orders;

public class OrderTests
{
    [Fact]
    public void Create_WithValidParameters_CreatesOrder()
    {
        // Arrange
        var customerName = "John Doe";
        var customerEmail = "john@example.com";
        var shippingAddress = "123 Main St";

        // Act
        var order = Order.Create(customerName, customerEmail, shippingAddress);

        // Assert
        Assert.NotNull(order);
        Assert.Equal(customerName, order.CustomerName);
        Assert.Equal(customerEmail, order.CustomerEmail);
        Assert.Equal(shippingAddress, order.ShippingAddress);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Empty(order.Items);
        Assert.Equal(0, order.TotalAmount);
        Assert.NotNull(order.Id);
        Assert.NotEqual(default(DateTime), order.OrderDate);
        Assert.NotEqual(default(DateTime), order.CreatedOnUtc);
        Assert.Null(order.ModifiedOnUtc);
    }

    [Fact]
    public void Create_WithEmptyCustomerName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Order.Create("", "john@example.com", "123 Main St"));
        Assert.Throws<ArgumentException>(() => Order.Create("   ", "john@example.com", "123 Main St"));
        Assert.Throws<ArgumentException>(() => Order.Create(null!, "john@example.com", "123 Main St"));
    }

    [Fact]
    public void Create_WithEmptyCustomerEmail_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Order.Create("John Doe", "", "123 Main St"));
    }

    [Fact]
    public void Create_WithEmptyShippingAddress_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Order.Create("John Doe", "john@example.com", ""));
    }

    [Fact]
    public void Create_RaisesOrderCreatedDomainEvent()
    {
        // Arrange
        var customerEmail = "john@example.com";

        // Act
        var order = Order.Create("John Doe", customerEmail, "123 Main St");

        // Assert
        var domainEvents = order.GetDomainEvents();
        Assert.Single(domainEvents);
        var createdEvent = Assert.IsType<OrderCreatedDomainEvent>(domainEvents[0]);
        Assert.Equal(order.Id, createdEvent.OrderId);
        Assert.Equal(customerEmail, createdEvent.CustomerEmail);
    }

    [Fact]
    public void AddItem_WhenPending_AddsItemToOrder()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");

        // Act
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);

        // Assert
        Assert.Single(order.Items);
        Assert.Equal("SKU-001", order.Items[0].ProductSku);
        Assert.Equal("Product 1", order.Items[0].ProductName);
        Assert.Equal(10.00m, order.Items[0].UnitPrice);
        Assert.Equal(2, order.Items[0].Quantity);
        Assert.Equal(20.00m, order.Items[0].TotalPrice);
        Assert.Equal(20.00m, order.TotalAmount);
    }

    [Fact]
    public void AddItem_WithSameSku_CombinesQuantities()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);

        // Act
        order.AddItem("SKU-001", "Product 1", 10.00m, 3);

        // Assert
        Assert.Single(order.Items);
        Assert.Equal(5, order.Items[0].Quantity);
        Assert.Equal(50.00m, order.TotalAmount);
    }

    [Fact]
    public void AddItem_WhenNotPending_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);
        order.ConfirmOrder();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.AddItem("SKU-002", "Product 2", 20.00m, 1));
    }

    [Fact]
    public void AddItem_RaisesOrderItemAddedDomainEvent()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.ClearDomainEvents();

        // Act
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);

        // Assert
        var domainEvents = order.GetDomainEvents();
        Assert.Single(domainEvents);
        var itemAddedEvent = Assert.IsType<OrderItemAddedDomainEvent>(domainEvents[0]);
        Assert.Equal(order.Id, itemAddedEvent.OrderId);
        Assert.Equal("SKU-001", itemAddedEvent.ProductSku);
        Assert.Equal(2, itemAddedEvent.Quantity);
    }

    [Fact]
    public void RemoveItem_WhenPending_RemovesItemFromOrder()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);
        order.AddItem("SKU-002", "Product 2", 20.00m, 1);

        // Act
        order.RemoveItem("SKU-001");

        // Assert
        Assert.Single(order.Items);
        Assert.Equal("SKU-002", order.Items[0].ProductSku);
        Assert.Equal(20.00m, order.TotalAmount);
    }

    [Fact]
    public void RemoveItem_WhenNotPending_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);
        order.ConfirmOrder();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.RemoveItem("SKU-001"));
    }

    [Fact]
    public void RemoveItem_WithNonExistentSku_ThrowsArgumentException()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => order.RemoveItem("SKU-999"));
    }

    [Fact]
    public void RemoveItem_RaisesOrderItemRemovedDomainEvent()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);
        order.ClearDomainEvents();

        // Act
        order.RemoveItem("SKU-001");

        // Assert
        var domainEvents = order.GetDomainEvents();
        Assert.Single(domainEvents);
        var itemRemovedEvent = Assert.IsType<OrderItemRemovedDomainEvent>(domainEvents[0]);
        Assert.Equal(order.Id, itemRemovedEvent.OrderId);
        Assert.Equal("SKU-001", itemRemovedEvent.ProductSku);
    }

    [Fact]
    public void ConfirmOrder_WhenPending_ChangesStatusToConfirmed()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);

        // Act
        order.ConfirmOrder();

        // Assert
        Assert.Equal(OrderStatus.Confirmed, order.Status);
        Assert.NotNull(order.ModifiedOnUtc);
    }

    [Fact]
    public void ConfirmOrder_WhenNotPending_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);
        order.ConfirmOrder();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.ConfirmOrder());
    }

    [Fact]
    public void ConfirmOrder_WithoutItems_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.ConfirmOrder());
    }

    [Fact]
    public void ConfirmOrder_RaisesOrderConfirmedDomainEvent()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);
        order.ClearDomainEvents();

        // Act
        order.ConfirmOrder();

        // Assert
        var domainEvents = order.GetDomainEvents();
        Assert.Single(domainEvents);
        var confirmedEvent = Assert.IsType<OrderConfirmedDomainEvent>(domainEvents[0]);
        Assert.Equal(order.Id, confirmedEvent.OrderId);
        Assert.Equal(20.00m, confirmedEvent.TotalAmount);
        Assert.Single(confirmedEvent.Items);
    }

    [Fact]
    public void ShipOrder_WhenConfirmed_ChangesStatusToShipped()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);
        order.ConfirmOrder();

        // Act
        order.ShipOrder();

        // Assert
        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.NotNull(order.ModifiedOnUtc);
    }

    [Fact]
    public void ShipOrder_WhenNotConfirmed_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.ShipOrder());
    }

    [Fact]
    public void ShipOrder_RaisesOrderShippedDomainEvent()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);
        order.ConfirmOrder();
        order.ClearDomainEvents();

        // Act
        order.ShipOrder();

        // Assert
        var domainEvents = order.GetDomainEvents();
        Assert.Single(domainEvents);
        var shippedEvent = Assert.IsType<OrderShippedDomainEvent>(domainEvents[0]);
        Assert.Equal(order.Id, shippedEvent.OrderId);
        Assert.Equal(order.CustomerEmail, shippedEvent.CustomerEmail);
        Assert.Equal(order.ShippingAddress, shippedEvent.ShippingAddress);
    }

    [Fact]
    public void DeliverOrder_WhenShipped_ChangesStatusToDelivered()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);
        order.ConfirmOrder();
        order.ShipOrder();

        // Act
        order.DeliverOrder();

        // Assert
        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.NotNull(order.ModifiedOnUtc);
    }

    [Fact]
    public void DeliverOrder_WhenNotShipped_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);
        order.ConfirmOrder();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.DeliverOrder());
    }

    [Fact]
    public void DeliverOrder_RaisesOrderDeliveredDomainEvent()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);
        order.ConfirmOrder();
        order.ShipOrder();
        order.ClearDomainEvents();

        // Act
        order.DeliverOrder();

        // Assert
        var domainEvents = order.GetDomainEvents();
        Assert.Single(domainEvents);
        var deliveredEvent = Assert.IsType<OrderDeliveredDomainEvent>(domainEvents[0]);
        Assert.Equal(order.Id, deliveredEvent.OrderId);
        Assert.Equal(order.CustomerEmail, deliveredEvent.CustomerEmail);
    }

    [Fact]
    public void CancelOrder_WhenPending_ChangesStatusToCancelled()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);

        // Act
        order.CancelOrder();

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.NotNull(order.ModifiedOnUtc);
    }

    [Fact]
    public void CancelOrder_WhenConfirmed_ChangesStatusToCancelled()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);
        order.ConfirmOrder();

        // Act
        order.CancelOrder();

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void CancelOrder_WhenShipped_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);
        order.ConfirmOrder();
        order.ShipOrder();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.CancelOrder());
    }

    [Fact]
    public void CancelOrder_WhenAlreadyCancelled_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);
        order.CancelOrder();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.CancelOrder());
    }

    [Fact]
    public void CancelOrder_RaisesOrderCancelledDomainEvent()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);
        order.ClearDomainEvents();

        // Act
        order.CancelOrder();

        // Assert
        var domainEvents = order.GetDomainEvents();
        Assert.Single(domainEvents);
        var cancelledEvent = Assert.IsType<OrderCancelledDomainEvent>(domainEvents[0]);
        Assert.Equal(order.Id, cancelledEvent.OrderId);
        Assert.Equal(OrderStatus.Pending, cancelledEvent.PreviousStatus);
        Assert.Single(cancelledEvent.Items);
    }

    [Fact]
    public void TotalAmount_WithMultipleItems_CalculatesCorrectTotal()
    {
        // Arrange
        var order = Order.Create("John Doe", "john@example.com", "123 Main St");
        order.AddItem("SKU-001", "Product 1", 10.00m, 2);
        order.AddItem("SKU-002", "Product 2", 20.00m, 1);
        order.AddItem("SKU-003", "Product 3", 5.00m, 3);

        // Assert
        Assert.Equal(55.00m, order.TotalAmount); // (10*2) + (20*1) + (5*3) = 55
    }
}

public class OrderItemTests
{
    [Fact]
    public void Create_WithValidParameters_CreatesOrderItem()
    {
        // Act
        var orderItem = OrderItem.Create("SKU-001", "Product 1", 10.00m, 2);

        // Assert
        Assert.Equal("SKU-001", orderItem.ProductSku);
        Assert.Equal("Product 1", orderItem.ProductName);
        Assert.Equal(10.00m, orderItem.UnitPrice);
        Assert.Equal(2, orderItem.Quantity);
        Assert.Equal(20.00m, orderItem.TotalPrice);
    }

    [Fact]
    public void Create_WithEmptyProductSku_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => OrderItem.Create("", "Product 1", 10.00m, 2));
    }

    [Fact]
    public void Create_WithEmptyProductName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => OrderItem.Create("SKU-001", "", 10.00m, 2));
    }

    [Fact]
    public void Create_WithZeroOrNegativeUnitPrice_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => OrderItem.Create("SKU-001", "Product 1", 0, 2));
        Assert.Throws<ArgumentException>(() => OrderItem.Create("SKU-001", "Product 1", -10.00m, 2));
    }

    [Fact]
    public void Create_WithZeroOrNegativeQuantity_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => OrderItem.Create("SKU-001", "Product 1", 10.00m, 0));
        Assert.Throws<ArgumentException>(() => OrderItem.Create("SKU-001", "Product 1", 10.00m, -2));
    }

    [Fact]
    public void TotalPrice_CalculatesCorrectly()
    {
        // Arrange
        var orderItem = OrderItem.Create("SKU-001", "Product 1", 10.50m, 3);

        // Assert
        Assert.Equal(31.50m, orderItem.TotalPrice);
    }
}
