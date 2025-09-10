using Shared.Errors;

namespace Clean.Architecture.Domain.Orders;

public static class OrderErrors
{
    public static readonly Error NotFound = new("Order.NotFound", "The order with the specified ID was not found.");

    public static readonly Error InvalidCustomerName = new("Order.InvalidCustomerName", "Customer name cannot be empty or whitespace.");

    public static readonly Error InvalidCustomerEmail = new("Order.InvalidCustomerEmail", "Customer email cannot be empty or whitespace.");

    public static readonly Error NoItems = new("Order.NoItems", "Order must contain at least one item.");

    public static readonly Error InvalidQuantity = new("Order.InvalidQuantity", "Order item quantity must be greater than zero.");

    public static readonly Error AlreadyConfirmed = new("Order.AlreadyConfirmed", "Order has already been confirmed and cannot be modified.");

    public static readonly Error AlreadyCancelled = new("Order.AlreadyCancelled", "Order has already been cancelled.");

    public static Error NotFoundWithId(Guid orderId) =>
        new("Order.NotFound", $"The order with ID '{orderId}' was not found.");

    public static Error ProductNotFoundForItem(Guid productId) =>
        new("Order.ProductNotFound", $"Product with ID '{productId}' was not found and cannot be added to the order.");

    public static Error ProductNotFoundForSku(string productSku) =>
        new("Order.ProductNotFound", $"Product with SKU '{productSku}' was not found and cannot be added to the order.");
}
