using Shared.Errors;

namespace Clean.Architecture.Domain.Products;

public static class ProductErrors
{
    public static readonly Error NotFound = new("Product.NotFound", "The product with the specified ID was not found.");

    public static readonly Error InvalidName = new("Product.InvalidName", "Product name cannot be empty or whitespace.");

    public static readonly Error InvalidPrice = new("Product.InvalidPrice", "Product price must be greater than zero.");

    public static readonly Error InvalidDescription = new("Product.InvalidDescription", "Product description cannot be empty or whitespace.");

    public static Error NotFoundWithId(Guid productId) =>
        new("Product.NotFound", $"The product with ID '{productId}' was not found.");
}
