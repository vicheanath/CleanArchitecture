using Clean.Architecture.Application.Products;
using Clean.Architecture.Domain.Inventory;
using Clean.Architecture.Domain.Orders;
using Clean.Architecture.Domain.Products;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;

namespace Clean.Architecture.Application.Orders.PlaceOrder;

/// <summary>
/// Handler for placing an order with product and inventory validation.
/// </summary>
internal sealed class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly Application.Products.IProductRepository _productRepository;
    private readonly IInventoryItemRepository _inventoryRepository;

    public PlaceOrderCommandHandler(
        IOrderRepository orderRepository,
        Application.Products.IProductRepository productRepository,
        IInventoryItemRepository inventoryRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
    }

    public async Task<Result<Guid>> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate that all products exist and are active
        var productValidationResults = new List<string>();
        var orderItemsData = new List<(Product Product, int Quantity)>();

        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetBySkuAsync(item.ProductSku, cancellationToken);
            if (product == null)
            {
                productValidationResults.Add($"Product with SKU '{item.ProductSku}' not found");
                continue;
            }

            if (!product.IsActive)
            {
                productValidationResults.Add($"Product '{product.Name}' (SKU: {item.ProductSku}) is not active");
                continue;
            }

            orderItemsData.Add((product, item.Quantity));
        }

        if (productValidationResults.Any())
        {
            return Result.Failure<Guid>(new ValidationError(productValidationResults));
        }

        // Validate inventory availability
        var inventoryValidationResults = new List<string>();

        foreach (var (product, quantity) in orderItemsData)
        {
            var inventoryItem = await _inventoryRepository.GetByProductSkuAsync(
                product.Sku,
                cancellationToken);

            if (inventoryItem == null)
            {
                inventoryValidationResults.Add($"No inventory found for product '{product.Name}' (SKU: {product.Sku})");
                continue;
            }

            if (inventoryItem.AvailableQuantity < quantity)
            {
                inventoryValidationResults.Add(
                    $"Insufficient stock for product '{product.Name}' (SKU: {product.Sku}). " +
                    $"Requested: {quantity}, Available: {inventoryItem.AvailableQuantity}");
            }
        }

        if (inventoryValidationResults.Any())
        {
            return Result.Failure<Guid>(new ValidationError(inventoryValidationResults));
        }

        // Create the order
        var order = Order.Create(
            request.CustomerName,
            request.CustomerEmail,
            request.ShippingAddress);

        // Add items to the order
        foreach (var (product, quantity) in orderItemsData)
        {
            order.AddItem(
                product.Sku,
                product.Name,
                product.ProductPricing.RegularPrice,
                quantity);
        }

        // Save the order
        await _orderRepository.AddAsync(order, cancellationToken);

        return Result.Success(order.Id.Value);
    }

    /// <summary>
    /// Validation error for order placement.
    /// </summary>
    private sealed class ValidationError : Shared.Errors.Error
    {
        public ValidationError(IEnumerable<string> errors)
            : base("Order.ValidationFailed", string.Join("; ", errors))
        {
        }
    }
}
