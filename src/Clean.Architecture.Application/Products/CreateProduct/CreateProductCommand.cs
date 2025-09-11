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
/// <param name="Brand">The product brand.</param>
/// <param name="Weight">The product weight.</param>
/// <param name="Dimensions">The product dimensions.</param>
/// <param name="Color">The product color.</param>
/// <param name="Size">The product size.</param>
/// <param name="SalePrice">The sale price.</param>
/// <param name="SaleStartDate">The sale start date.</param>
/// <param name="SaleEndDate">The sale end date.</param>
/// <param name="MetaTitle">The meta title for SEO.</param>
/// <param name="MetaDescription">The meta description for SEO.</param>
/// <param name="RequiresShipping">Whether the product requires shipping.</param>
/// <param name="ShippingWeight">The shipping weight.</param>
/// <param name="IsFeatured">Whether the product is featured.</param>
/// <param name="SortOrder">The sort order.</param>
/// <param name="Images">The product images.</param>
/// <param name="Tags">The product tags.</param>
public record CreateProductCommand(
    string Sku,
    string Name,
    string Description,
    decimal Price,
    string Category,
    string Brand = "",
    decimal Weight = 0,
    string Dimensions = "",
    string Color = "",
    string Size = "",
    decimal? SalePrice = null,
    DateTime? SaleStartDate = null,
    DateTime? SaleEndDate = null,
    string MetaTitle = "",
    string MetaDescription = "",
    bool RequiresShipping = true,
    decimal ShippingWeight = 0,
    bool IsFeatured = false,
    int SortOrder = 0,
    List<string>? Images = null,
    List<string>? Tags = null) : ICommand<CreateProductResult>;
