using Shared.Primitives;

namespace Clean.Architecture.Domain.Products;

/// <summary>
/// Domain event raised when a product is created.
/// </summary>
/// <param name="Id">The event identifier.</param>
/// <param name="OccurredOnUtc">The date and time the event occurred.</param>
/// <param name="ProductId">The product identifier.</param>
/// <param name="Sku">The product SKU.</param>
/// <param name="Name">The product name.</param>
/// <param name="Category">The product category.</param>
/// <param name="Price">The product price.</param>
public sealed record ProductCreatedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    ProductId ProductId,
    string Sku,
    string Name,
    string Category,
    decimal Price) : DomainEvent(Id, OccurredOnUtc);

/// <summary>
/// Domain event raised when a product price is updated.
/// </summary>
/// <param name="Id">The event identifier.</param>
/// <param name="OccurredOnUtc">The date and time the event occurred.</param>
/// <param name="ProductId">The product identifier.</param>
/// <param name="Sku">The product SKU.</param>
/// <param name="OldPrice">The old price.</param>
/// <param name="NewPrice">The new price.</param>
public sealed record ProductPriceUpdatedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    ProductId ProductId,
    string Sku,
    decimal OldPrice,
    decimal NewPrice) : DomainEvent(Id, OccurredOnUtc);

/// <summary>
/// Domain event raised when a product is activated.
/// </summary>
/// <param name="Id">The event identifier.</param>
/// <param name="OccurredOnUtc">The date and time the event occurred.</param>
/// <param name="ProductId">The product identifier.</param>
/// <param name="Sku">The product SKU.</param>
public sealed record ProductActivatedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    ProductId ProductId,
    string Sku) : DomainEvent(Id, OccurredOnUtc);

/// <summary>
/// Domain event raised when a product is deactivated.
/// </summary>
/// <param name="Id">The event identifier.</param>
/// <param name="OccurredOnUtc">The date and time the event occurred.</param>
/// <param name="ProductId">The product identifier.</param>
/// <param name="Sku">The product SKU.</param>
public sealed record ProductDeactivatedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    ProductId ProductId,
    string Sku) : DomainEvent(Id, OccurredOnUtc);

/// <summary>
/// Domain event raised when a product information is updated.
/// </summary>
/// <param name="Id">The event identifier.</param>
/// <param name="OccurredOnUtc">The date and time the event occurred.</param>
/// <param name="ProductId">The product identifier.</param>
/// <param name="Sku">The product SKU.</param>
/// <param name="Name">The updated name.</param>
/// <param name="Category">The updated category.</param>
public sealed record ProductInfoUpdatedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    ProductId ProductId,
    string Sku,
    string Name,
    string Category) : DomainEvent(Id, OccurredOnUtc);

/// <summary>
/// Domain event raised when a product sale information is updated.
/// </summary>
/// <param name="Id">The event identifier.</param>
/// <param name="OccurredOnUtc">The date and time the event occurred.</param>
/// <param name="ProductId">The product identifier.</param>
/// <param name="Sku">The product SKU.</param>
/// <param name="SalePrice">The sale price.</param>
/// <param name="SaleStartDate">The sale start date.</param>
/// <param name="SaleEndDate">The sale end date.</param>
public sealed record ProductSaleUpdatedDomainEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    ProductId ProductId,
    string Sku,
    decimal? SalePrice,
    DateTime? SaleStartDate,
    DateTime? SaleEndDate) : DomainEvent(Id, OccurredOnUtc);
