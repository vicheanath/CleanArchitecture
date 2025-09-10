using Shared.Primitives;

namespace Clean.Architecture.Domain.Orders;

public sealed class OrderId : IEntityId
{
    public OrderId(Guid value) => Value = value;
    public Guid Value { get; }
    public static OrderId New() => new(Guid.NewGuid());
    public static OrderId Create(Guid value) => new(value);

    public override bool Equals(object? obj)
    {
        return obj is OrderId other && Value.Equals(other.Value);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    public static bool operator ==(OrderId? left, OrderId? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(OrderId? left, OrderId? right)
    {
        return !Equals(left, right);
    }
}

public sealed class OrderItem : ValueObject
{
    private OrderItem(string productSku, string productName, decimal unitPrice, int quantity)
    {
        ProductSku = productSku;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    /// <summary>
    /// Gets the product SKU for inventory integration.
    /// </summary>
    public string ProductSku { get; private set; }

    /// <summary>
    /// Gets the product name.
    /// </summary>
    public string ProductName { get; private set; }

    /// <summary>
    /// Gets the unit price at the time of order.
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Gets the quantity ordered.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Gets the total price for this line item.
    /// </summary>
    public decimal TotalPrice => UnitPrice * Quantity;

    /// <summary>
    /// Creates a new order item.
    /// </summary>
    /// <param name="productSku">The product SKU.</param>
    /// <param name="productName">The product name.</param>
    /// <param name="unitPrice">The unit price.</param>
    /// <param name="quantity">The quantity.</param>
    /// <returns>A new order item.</returns>
    public static OrderItem Create(string productSku, string productName, decimal unitPrice, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productSku))
            throw new ArgumentException("Product SKU cannot be empty", nameof(productSku));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty", nameof(productName));

        if (unitPrice <= 0)
            throw new ArgumentException("Unit price must be greater than zero", nameof(unitPrice));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        return new OrderItem(productSku, productName, unitPrice, quantity);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return ProductSku;
        yield return ProductName;
        yield return UnitPrice;
        yield return Quantity;
    }
}

public enum OrderStatus
{
    /// <summary>
    /// Order is pending confirmation and inventory reservation.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Order is confirmed with inventory reserved.
    /// </summary>
    Confirmed = 2,

    /// <summary>
    /// Order is being shipped.
    /// </summary>
    Shipped = 3,

    /// <summary>
    /// Order has been delivered.
    /// </summary>
    Delivered = 4,

    /// <summary>
    /// Order has been cancelled.
    /// </summary>
    Cancelled = 5
}

public sealed class Order : Entity<OrderId>, IAuditable
{
    private readonly List<OrderItem> _items = new();

    private Order(OrderId id, string customerName, string customerEmail, string shippingAddress)
        : base(id)
    {
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        ShippingAddress = shippingAddress;
        Status = OrderStatus.Pending;
        OrderDate = DateTime.UtcNow;
        CreatedOnUtc = DateTime.UtcNow;
        ModifiedOnUtc = null;
    }

    private Order() { }

    /// <summary>
    /// Gets the customer name.
    /// </summary>
    public string CustomerName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the customer email.
    /// </summary>
    public string CustomerEmail { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the shipping address.
    /// </summary>
    public string ShippingAddress { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the order status.
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// Gets the order date.
    /// </summary>
    public DateTime OrderDate { get; private set; }

    /// <summary>
    /// Gets the read-only collection of order items.
    /// </summary>
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Gets the total amount of the order.
    /// </summary>
    public decimal TotalAmount => _items.Sum(item => item.TotalPrice);

    /// <inheritdoc />
    public DateTime CreatedOnUtc { get; private set; }

    /// <inheritdoc />
    public DateTime? ModifiedOnUtc { get; private set; }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="customerName">The customer name.</param>
    /// <param name="customerEmail">The customer email.</param>
    /// <param name="shippingAddress">The shipping address.</param>
    /// <returns>A new order.</returns>
    public static Order Create(string customerName, string customerEmail, string shippingAddress)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name cannot be empty", nameof(customerName));

        if (string.IsNullOrWhiteSpace(customerEmail))
            throw new ArgumentException("Customer email cannot be empty", nameof(customerEmail));

        if (string.IsNullOrWhiteSpace(shippingAddress))
            throw new ArgumentException("Shipping address cannot be empty", nameof(shippingAddress));

        var order = new Order(OrderId.New(), customerName, customerEmail, shippingAddress);

        order.RaiseDomainEvent(new OrderCreatedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            order.Id,
            order.CustomerEmail));

        return order;
    }

    /// <summary>
    /// Adds an item to the order. Only allowed when status is Pending.
    /// </summary>
    /// <param name="productSku">The product SKU.</param>
    /// <param name="productName">The product name.</param>
    /// <param name="unitPrice">The unit price.</param>
    /// <param name="quantity">The quantity.</param>
    public void AddItem(string productSku, string productName, decimal unitPrice, int quantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot add items to a non-pending order");

        var existingItem = _items.FirstOrDefault(i => i.ProductSku == productSku);
        if (existingItem != null)
        {
            // Remove existing item and add new one with combined quantity
            _items.Remove(existingItem);
            quantity += existingItem.Quantity;
        }

        var orderItem = OrderItem.Create(productSku, productName, unitPrice, quantity);
        _items.Add(orderItem);

        ModifiedOnUtc = DateTime.UtcNow;

        RaiseDomainEvent(new OrderItemAddedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            productSku,
            quantity));
    }

    /// <summary>
    /// Confirms the order and transitions to confirmed status.
    /// This should be called after inventory reservation is successful.
    /// </summary>
    public void ConfirmOrder()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Can only confirm pending orders");

        if (!_items.Any())
            throw new InvalidOperationException("Cannot confirm order without items");

        Status = OrderStatus.Confirmed;
        ModifiedOnUtc = DateTime.UtcNow;

        RaiseDomainEvent(new OrderConfirmedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            TotalAmount,
            _items.Select(i => new OrderItemSummary(i.ProductSku, i.Quantity)).ToList()));
    }

    /// <summary>
    /// Ships the order.
    /// </summary>
    public void ShipOrder()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Can only ship confirmed orders");

        Status = OrderStatus.Shipped;
        ModifiedOnUtc = DateTime.UtcNow;

        RaiseDomainEvent(new OrderShippedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            CustomerEmail,
            ShippingAddress));
    }

    /// <summary>
    /// Delivers the order.
    /// </summary>
    public void DeliverOrder()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Can only deliver shipped orders");

        Status = OrderStatus.Delivered;
        ModifiedOnUtc = DateTime.UtcNow;

        RaiseDomainEvent(new OrderDeliveredDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            CustomerEmail));
    }

    /// <summary>
    /// Cancels the order. Only allowed for Pending or Confirmed orders.
    /// </summary>
    public void CancelOrder()
    {
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled");

        if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel shipped or delivered orders");

        var previousStatus = Status;
        Status = OrderStatus.Cancelled;
        ModifiedOnUtc = DateTime.UtcNow;

        RaiseDomainEvent(new OrderCancelledDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            previousStatus,
            _items.Select(i => new OrderItemSummary(i.ProductSku, i.Quantity)).ToList()));
    }

    /// <summary>
    /// Removes an item from the order. Only allowed when status is Pending.
    /// </summary>
    /// <param name="productSku">The product SKU to remove.</param>
    public void RemoveItem(string productSku)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot remove items from a non-pending order");

        var item = _items.FirstOrDefault(i => i.ProductSku == productSku);
        if (item == null)
            throw new ArgumentException($"Order item with SKU '{productSku}' not found", nameof(productSku));

        _items.Remove(item);
        ModifiedOnUtc = DateTime.UtcNow;

        RaiseDomainEvent(new OrderItemRemovedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            productSku));
    }
}
