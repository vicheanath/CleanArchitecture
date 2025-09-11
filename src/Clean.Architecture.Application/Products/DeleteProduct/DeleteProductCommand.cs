using Shared.Messaging;

namespace Clean.Architecture.Application.Products.DeleteProduct;

/// <summary>
/// Command to delete a product.
/// </summary>
/// <param name="Id">The product identifier.</param>
public record DeleteProductCommand(Guid Id) : ICommand;
