using Clean.Architecture.Application.Products.DTOs;
using Shared.Messaging;

namespace Clean.Architecture.Application.Products.CreateProduct;

/// <summary>
/// Command to create a new product with automatic inventory setup.
/// </summary>
/// <param name="Sku">The product SKU.</param>
/// <param name="Name">The product name.</param>
/// <param name="Description">The product description.</param>
/// <param name="Price">The product price.</param>
/// <param name="Category">The product category.</param>
public record CreateProductCommand(
    string Sku,
    string Name,
    string Description,
    decimal Price,
    string Category) : ICommand<CreateProductResult>;
