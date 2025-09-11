using Shared.Primitives;
using Clean.Architecture.Domain.Products.ValueObjects;

namespace Clean.Architecture.Domain.Products;

/// <summary>
/// Represents the product identifier.
/// </summary>
/// <param name="Value">The identifier value.</param>
public sealed record ProductId(Guid Value) : IEntityId
{
    /// <summary>
    /// Parameterless constructor for serialization
    /// </summary>
    public ProductId() : this(Guid.Empty) { }

    public static ProductId New() => new(Guid.NewGuid());
    public static ProductId Create(Guid value) => new(value);
}

public sealed class Product : Entity<ProductId>, IAuditable
{
    private Product(ProductId id, string sku, string name, string description, string category,
        string brand, Pricing pricing, PhysicalAttributes physicalAttributes, SeoMetadata seoMetadata,
        ShippingInfo shippingInfo, bool isFeatured, int sortOrder, ProductImages images, ProductTags tags)
        : base(id)
    {
        Sku = sku;
        Name = name;
        Description = description;
        Category = category;
        Brand = brand;
        ProductPricing = pricing;
        ProductPhysicalAttributes = physicalAttributes;
        ProductSeoMetadata = seoMetadata;
        ProductShippingInfo = shippingInfo;
        IsFeatured = isFeatured;
        SortOrder = sortOrder;
        Images = images;
        Tags = tags;
        IsActive = true;
        CreatedOnUtc = DateTime.UtcNow;
        ModifiedOnUtc = null;
    }


    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Product() : base(ProductId.New())
    {
        // EF Core constructor - initialize with safe defaults
        Sku = string.Empty;
        Name = string.Empty;
        Description = string.Empty;
        Category = string.Empty;
        Brand = string.Empty;

        // Create safe default value objects for EF Core
        ProductPricing = Pricing.Create(1); // EF Core will ignore this anyway
        ProductPhysicalAttributes = PhysicalAttributes.Create();
        ProductSeoMetadata = SeoMetadata.Create(string.Empty, string.Empty);
        ProductShippingInfo = ShippingInfo.Create(false, 0);
        Images = ProductImages.Create(new List<string>());
        Tags = ProductTags.Create(new List<string>());

        IsFeatured = false;
        SortOrder = 0;
        IsActive = false;
        CreatedOnUtc = DateTime.UtcNow;
        ModifiedOnUtc = null;
    }

    /// <summary>
    /// Constructor for deserialization
    /// </summary>
    public Product(ProductId id, string sku, string name, string description, string category,
        string brand, Pricing pricing, PhysicalAttributes physicalAttributes, SeoMetadata seoMetadata,
        ShippingInfo shippingInfo, bool isFeatured, int sortOrder, bool isActive,
        DateTime createdOnUtc, DateTime? modifiedOnUtc, ProductImages images, ProductTags tags)
        : base(id)
    {
        Sku = sku;
        Name = name;
        Description = description;
        Category = category;
        Brand = brand;
        ProductPricing = pricing;
        ProductPhysicalAttributes = physicalAttributes;
        ProductSeoMetadata = seoMetadata;
        ProductShippingInfo = shippingInfo;
        IsFeatured = isFeatured;
        SortOrder = sortOrder;
        IsActive = isActive;
        CreatedOnUtc = createdOnUtc;
        ModifiedOnUtc = modifiedOnUtc;
        Images = images;
        Tags = tags;
    }

    /// <summary>
    /// Gets the product SKU (Stock Keeping Unit) - unique identifier for inventory.
    /// </summary>
    public string Sku { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the product name.
    /// </summary>
    public string Name { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the product description.
    /// </summary>
    public string Description { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the product category.
    /// </summary>
    public string Category { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the product brand.
    /// </summary>
    public string Brand { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the product pricing information.
    /// </summary>
    public Pricing ProductPricing { get; internal set; }

    /// <summary>
    /// Gets the product physical attributes.
    /// </summary>
    public PhysicalAttributes ProductPhysicalAttributes { get; internal set; }

    /// <summary>
    /// Gets the product SEO metadata.
    /// </summary>
    public SeoMetadata ProductSeoMetadata { get; internal set; }

    /// <summary>
    /// Gets the product shipping information.
    /// </summary>
    public ShippingInfo ProductShippingInfo { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the product is featured.
    /// </summary>
    public bool IsFeatured { get; internal set; }

    /// <summary>
    /// Gets the sort order for display.
    /// </summary>
    public int SortOrder { get; internal set; }

    /// <summary>
    /// Gets the product images.
    /// </summary>
    public ProductImages Images { get; internal set; }

    /// <summary>
    /// Gets the product tags.
    /// </summary>
    public ProductTags Tags { get; internal set; }

    /// <summary>
    /// Gets the current effective price (sale price if on sale and valid, otherwise regular price).
    /// </summary>
    public decimal EffectivePrice => ProductPricing.EffectivePrice;

    /// <summary>
    /// Gets a value indicating whether the product is currently on sale.
    /// </summary>
    public bool IsOnSale => ProductPricing.IsOnSale;

    /// <summary>
    /// Gets a value indicating whether the product is active and available for sale.
    /// </summary>
    public bool IsActive { get; internal set; }

    /// <inheritdoc />
    public DateTime CreatedOnUtc { get; internal set; }

    /// <inheritdoc />
    public DateTime? ModifiedOnUtc { get; internal set; }

    /// <summary>
    /// Creates a new product with inventory integration.
    /// </summary>
    /// <param name="sku">The product SKU.</param>
    /// <param name="name">The product name.</param>
    /// <param name="description">The product description.</param>
    /// <param name="price">The product price.</param>
    /// <param name="category">The product category.</param>
    /// <param name="brand">The product brand.</param>
    /// <param name="weight">The product weight.</param>
    /// <param name="dimensions">The product dimensions.</param>
    /// <param name="color">The product color.</param>
    /// <param name="size">The product size.</param>
    /// <param name="salePrice">The sale price.</param>
    /// <param name="saleStartDate">The sale start date.</param>
    /// <param name="saleEndDate">The sale end date.</param>
    /// <param name="metaTitle">The meta title for SEO.</param>
    /// <param name="metaDescription">The meta description for SEO.</param>
    /// <param name="requiresShipping">Whether the product requires shipping.</param>
    /// <param name="shippingWeight">The shipping weight.</param>
    /// <param name="isFeatured">Whether the product is featured.</param>
    /// <param name="sortOrder">The sort order.</param>
    /// <param name="images">The product images.</param>
    /// <param name="tags">The product tags.</param>
    /// <returns>The newly created product.</returns>
    public static Product Create(string sku, string name, string description, decimal price, string category,
        string brand = "", decimal weight = 0, string dimensions = "", string color = "", string size = "",
        decimal? salePrice = null, DateTime? saleStartDate = null, DateTime? saleEndDate = null,
        string metaTitle = "", string metaDescription = "", bool requiresShipping = true,
        decimal shippingWeight = 0, bool isFeatured = false, int sortOrder = 0,
        List<string>? images = null, List<string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("Product SKU cannot be empty", nameof(sku));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        if (price <= 0)
            throw new ArgumentException("Product price must be greater than zero", nameof(price));

        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Product category cannot be empty", nameof(category));

        if (salePrice.HasValue && salePrice.Value <= 0)
            throw new ArgumentException("Sale price must be greater than zero if specified", nameof(salePrice));

        if (salePrice.HasValue && salePrice.Value >= price)
            throw new ArgumentException("Sale price must be less than regular price", nameof(salePrice));

        if (saleStartDate.HasValue && saleEndDate.HasValue && saleStartDate.Value >= saleEndDate.Value)
            throw new ArgumentException("Sale start date must be before sale end date", nameof(saleStartDate));

        // Create value objects
        var pricing = salePrice.HasValue
            ? Pricing.CreateWithSale(price, salePrice.Value, saleStartDate, saleEndDate)
            : Pricing.Create(price);
        var physicalAttributes = PhysicalAttributes.Create(weight, dimensions, color, size);
        var seoMetadata = SeoMetadata.Create(string.IsNullOrWhiteSpace(metaTitle) ? name : metaTitle,
            string.IsNullOrWhiteSpace(metaDescription) ? description : metaDescription);
        var shippingInfo = ShippingInfo.Create(requiresShipping, shippingWeight);
        var productImages = ProductImages.Create(images ?? new List<string>());
        var productTags = ProductTags.Create(tags ?? new List<string>());

        var product = new Product(ProductId.New(), sku, name, description, category, brand,
            pricing, physicalAttributes, seoMetadata, shippingInfo,
            isFeatured, sortOrder, productImages, productTags);

        product.RaiseDomainEvent(new ProductCreatedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            product.Id,
            product.Sku,
            product.Name,
            product.Category,
            product.ProductPricing.RegularPrice));

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

        decimal oldPrice = ProductPricing.RegularPrice;
        ProductPricing = Pricing.Create(newPrice);
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
    /// <param name="brand">The new brand.</param>
    /// <param name="weight">The new weight.</param>
    /// <param name="dimensions">The new dimensions.</param>
    /// <param name="color">The new color.</param>
    /// <param name="size">The new size.</param>
    /// <param name="metaTitle">The new meta title.</param>
    /// <param name="metaDescription">The new meta description.</param>
    /// <param name="requiresShipping">Whether the product requires shipping.</param>
    /// <param name="shippingWeight">The new shipping weight.</param>
    /// <param name="isFeatured">Whether the product is featured.</param>
    /// <param name="sortOrder">The new sort order.</param>
    public void UpdateInfo(string name, string description, string category, string brand = "",
        decimal weight = 0, string dimensions = "", string color = "", string size = "",
        string metaTitle = "", string metaDescription = "", bool requiresShipping = true,
        decimal shippingWeight = 0, bool isFeatured = false, int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Product category cannot be empty", nameof(category));

        Name = name;
        Description = description;
        Category = category;
        Brand = brand ?? string.Empty;

        // Update value objects
        ProductPhysicalAttributes = PhysicalAttributes.Create(weight, dimensions, color, size);
        ProductSeoMetadata = SeoMetadata.Create(
            string.IsNullOrWhiteSpace(metaTitle) ? name : metaTitle,
            string.IsNullOrWhiteSpace(metaDescription) ? description : metaDescription);
        ProductShippingInfo = ShippingInfo.Create(requiresShipping, shippingWeight);

        IsFeatured = isFeatured;
        SortOrder = sortOrder;
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
    /// Updates the product sale information.
    /// </summary>
    /// <param name="salePrice">The sale price.</param>
    /// <param name="saleStartDate">The sale start date.</param>
    /// <param name="saleEndDate">The sale end date.</param>
    public void UpdateSaleInfo(decimal? salePrice, DateTime? saleStartDate = null, DateTime? saleEndDate = null)
    {
        if (salePrice.HasValue && salePrice.Value <= 0)
            throw new ArgumentException("Sale price must be greater than zero if specified", nameof(salePrice));

        if (salePrice.HasValue && salePrice.Value >= ProductPricing.RegularPrice)
            throw new ArgumentException("Sale price must be less than regular price", nameof(salePrice));

        if (saleStartDate.HasValue && saleEndDate.HasValue && saleStartDate.Value >= saleEndDate.Value)
            throw new ArgumentException("Sale start date must be before sale end date", nameof(saleStartDate));

        // Update pricing with sale information
        ProductPricing = salePrice.HasValue
            ? Pricing.CreateWithSale(ProductPricing.RegularPrice, salePrice.Value, saleStartDate, saleEndDate)
            : Pricing.Create(ProductPricing.RegularPrice);

        ModifiedOnUtc = DateTime.UtcNow;

        // Raise domain event for sale update
        RaiseDomainEvent(new ProductSaleUpdatedDomainEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Id,
            Sku,
            salePrice,
            saleStartDate,
            saleEndDate));
    }

    /// <summary>
    /// Adds an image to the product.
    /// </summary>
    /// <param name="imageUrl">The image URL.</param>
    public void AddImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL cannot be empty", nameof(imageUrl));

        var updatedImages = Images.AddImage(imageUrl);
        if (updatedImages != Images)
        {
            Images = updatedImages;
            ModifiedOnUtc = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Removes an image from the product.
    /// </summary>
    /// <param name="imageUrl">The image URL to remove.</param>
    public void RemoveImage(string imageUrl)
    {
        var updatedImages = Images.RemoveImage(imageUrl);
        if (updatedImages != Images)
        {
            Images = updatedImages;
            ModifiedOnUtc = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Adds a tag to the product.
    /// </summary>
    /// <param name="tag">The tag to add.</param>
    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Tag cannot be empty", nameof(tag));

        var updatedTags = Tags.AddTag(tag);
        if (updatedTags != Tags)
        {
            Tags = updatedTags;
            ModifiedOnUtc = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Removes a tag from the product.
    /// </summary>
    /// <param name="tag">The tag to remove.</param>
    public void RemoveTag(string tag)
    {
        var updatedTags = Tags.RemoveTag(tag);
        if (updatedTags != Tags)
        {
            Tags = updatedTags;
            ModifiedOnUtc = DateTime.UtcNow;
        }
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
