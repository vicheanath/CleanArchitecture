using Shared.Primitives;

namespace Clean.Architecture.Domain.Products;

public sealed class ProductId : IEntityId
{
    public ProductId(Guid value) => Value = value;

    /// <summary>
    /// Parameterless constructor for serialization
    /// </summary>
    public ProductId() => Value = Guid.Empty;

    public Guid Value { get; set; }
    public static ProductId New() => new(Guid.NewGuid());
    public static ProductId Create(Guid value) => new(value);

    public override bool Equals(object? obj)
    {
        return obj is ProductId other && Value.Equals(other.Value);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    public static bool operator ==(ProductId? left, ProductId? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ProductId? left, ProductId? right)
    {
        return !Equals(left, right);
    }
}

public sealed class Product : Entity<ProductId>, IAuditable
{
    private Product(ProductId id, string sku, string name, string description, decimal price, string category)
        : base(id)
    {
        Sku = sku;
        Name = name;
        Description = description;
        Price = price;
        Category = category;
        IsActive = true;
        CreatedOnUtc = DateTime.UtcNow;
        ModifiedOnUtc = null;
    }

    private Product() { }

    /// <summary>
    /// Constructor for deserialization
    /// </summary>
    public Product(ProductId id, string sku, string name, string description, decimal price, string category, bool isActive, DateTime createdOnUtc, DateTime? modifiedOnUtc)
        : base(id)
    {
        Sku = sku;
        Name = name;
        Description = description;
        Price = price;
        Category = category;
        IsActive = isActive;
        CreatedOnUtc = createdOnUtc;
        ModifiedOnUtc = modifiedOnUtc;
    }

    /// <summary>
    /// Gets the product SKU (Stock Keeping Unit) - unique identifier for inventory.
    /// </summary>
    public string Sku { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the product name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the product description.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the product price.
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// Gets the product category.
    /// </summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the product is active and available for sale.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <inheritdoc />
    public DateTime CreatedOnUtc { get; private set; }

    /// <inheritdoc />
    public DateTime? ModifiedOnUtc { get; private set; }

    /// <summary>
    /// Creates a new product with inventory integration.
    /// </summary>
    /// <param name="sku">The product SKU.</param>
    /// <param name="name">The product name.</param>
    /// <param name="description">The product description.</param>
    /// <param name="price">The product price.</param>
    /// <param name="category">The product category.</param>
    /// <returns>The newly created product.</returns>
    public static Product Create(string sku, string name, string description, decimal price, string category)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("Product SKU cannot be empty", nameof(sku));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        if (price <= 0)
            throw new ArgumentException("Product price must be greater than zero", nameof(price));

        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Product category cannot be empty", nameof(category));

        var product = new Product(ProductId.New(), sku, name, description, price, category);

        product.RaiseDomainEvent(new ProductCreatedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            product.Id,
            product.Sku,
            product.Name,
            product.Category,
            product.Price));

        return product;
    }

    /// <summary>
    /// Updates the product price.
    /// </summary>
    /// <param name="newPrice">The new price.</param>
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new ArgumentException("Product price must be greater than zero", nameof(newPrice));

        decimal oldPrice = Price;
        Price = newPrice;
        ModifiedOnUtc = DateTime.UtcNow;

        RaiseDomainEvent(new ProductPriceUpdatedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            Sku,
            oldPrice,
            newPrice));
    }

    /// <summary>
    /// Updates the product information.
    /// </summary>
    /// <param name="name">The new name.</param>
    /// <param name="description">The new description.</param>
    /// <param name="category">The new category.</param>
    public void UpdateInfo(string name, string description, string category)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Product category cannot be empty", nameof(category));

        Name = name;
        Description = description;
        Category = category;
        ModifiedOnUtc = DateTime.UtcNow;

        RaiseDomainEvent(new ProductInfoUpdatedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            Sku,
            name,
            category));
    }

    /// <summary>
    /// Activates the product for sale.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        ModifiedOnUtc = DateTime.UtcNow;

        RaiseDomainEvent(new ProductActivatedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            Sku));
    }

    /// <summary>
    /// Deactivates the product from sale.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        ModifiedOnUtc = DateTime.UtcNow;

        RaiseDomainEvent(new ProductDeactivatedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            Sku));
    }
}
