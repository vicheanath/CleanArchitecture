using Shared.Messaging;

namespace Clean.Architecture.Application.Orders.PlaceOrder;

/// <summary>
/// Represents an item to be added to an order.
/// </summary>
/// <param name="ProductSku">The product SKU.</param>
/// <param name="Quantity">The quantity to order.</param>
public sealed record PlaceOrderItem(string ProductSku, int Quantity);

/// <summary>
/// Command to place a new order with inventory validation.
/// </summary>
/// <param name="CustomerName">The customer name.</param>
/// <param name="CustomerEmail">The customer email.</param>
/// <param name="ShippingAddress">The shipping address.</param>
/// <param name="Items">The items to order.</param>
public sealed record PlaceOrderCommand(
    string CustomerName,
    string CustomerEmail,
    string ShippingAddress,
    IReadOnlyList<PlaceOrderItem> Items) : ICommand<Guid>;
