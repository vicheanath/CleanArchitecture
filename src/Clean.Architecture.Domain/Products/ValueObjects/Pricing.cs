using Shared.Primitives;

namespace Clean.Architecture.Domain.Products.ValueObjects;

/// <summary>
/// Represents the pricing information for a product.
/// </summary>
public sealed class Pricing : ValueObject
{
    /// <summary>
    /// Gets the regular price of the product.
    /// </summary>
    public decimal RegularPrice { get; }

    /// <summary>
    /// Gets the sale price of the product, if any.
    /// </summary>
    public decimal? SalePrice { get; }

    /// <summary>
    /// Gets the sale start date, if any.
    /// </summary>
    public DateTime? SaleStartDate { get; }

    /// <summary>
    /// Gets the sale end date, if any.
    /// </summary>
    public DateTime? SaleEndDate { get; }

    /// <summary>
    /// Gets the effective price (sale price if active, otherwise regular price).
    /// </summary>
    public decimal EffectivePrice => IsOnSale ? SalePrice!.Value : RegularPrice;

    /// <summary>
    /// Gets a value indicating whether the product is currently on sale.
    /// </summary>
    public bool IsOnSale
    {
        get
        {
            if (!SalePrice.HasValue) return false;

            var now = DateTime.UtcNow;

            // Check if sale has started (if start date is specified)
            if (SaleStartDate.HasValue && now < SaleStartDate.Value) return false;

            // Check if sale has ended (if end date is specified)
            if (SaleEndDate.HasValue && now > SaleEndDate.Value) return false;

            return true;
        }
    }

    /// <summary>
    /// Gets the discount percentage if the product is on sale.
    /// </summary>
    public decimal? DiscountPercentage
    {
        get
        {
            if (!IsOnSale || RegularPrice == 0) return null;
            return Math.Round((RegularPrice - SalePrice!.Value) / RegularPrice * 100, 2);
        }
    }

    private Pricing(decimal regularPrice, decimal? salePrice, DateTime? saleStartDate, DateTime? saleEndDate)
    {
        RegularPrice = regularPrice;
        SalePrice = salePrice;
        SaleStartDate = saleStartDate;
        SaleEndDate = saleEndDate;
    }

    /// <summary>
    /// Creates a new pricing instance with only regular price.
    /// </summary>
    /// <param name="regularPrice">The regular price.</param>
    /// <returns>A new pricing instance.</returns>
    public static Pricing Create(decimal regularPrice)
    {
        if (regularPrice <= 0)
            throw new ArgumentException("Regular price must be greater than zero.", nameof(regularPrice));

        return new Pricing(regularPrice, null, null, null);
    }

    /// <summary>
    /// Creates a new pricing instance with sale information.
    /// </summary>
    /// <param name="regularPrice">The regular price.</param>
    /// <param name="salePrice">The sale price.</param>
    /// <param name="saleStartDate">The sale start date.</param>
    /// <param name="saleEndDate">The sale end date.</param>
    /// <returns>A new pricing instance.</returns>
    public static Pricing CreateWithSale(decimal regularPrice, decimal salePrice, DateTime? saleStartDate = null, DateTime? saleEndDate = null)
    {
        if (regularPrice <= 0)
            throw new ArgumentException("Regular price must be greater than zero.", nameof(regularPrice));

        if (salePrice <= 0)
            throw new ArgumentException("Sale price must be greater than zero.", nameof(salePrice));

        if (salePrice >= regularPrice)
            throw new ArgumentException("Sale price must be less than regular price.", nameof(salePrice));

        if (saleStartDate.HasValue && saleEndDate.HasValue && saleEndDate <= saleStartDate)
            throw new ArgumentException("Sale end date must be after sale start date.", nameof(saleEndDate));

        return new Pricing(regularPrice, salePrice, saleStartDate, saleEndDate);
    }

    /// <summary>
    /// Updates the regular price.
    /// </summary>
    /// <param name="newRegularPrice">The new regular price.</param>
    /// <returns>A new pricing instance with updated regular price.</returns>
    public Pricing UpdateRegularPrice(decimal newRegularPrice)
    {
        if (newRegularPrice <= 0)
            throw new ArgumentException("Regular price must be greater than zero.", nameof(newRegularPrice));

        // If there's a sale price, ensure it's still valid
        if (SalePrice.HasValue && SalePrice >= newRegularPrice)
            throw new ArgumentException("New regular price must be greater than current sale price.", nameof(newRegularPrice));

        return new Pricing(newRegularPrice, SalePrice, SaleStartDate, SaleEndDate);
    }

    /// <summary>
    /// Adds or updates sale information.
    /// </summary>
    /// <param name="salePrice">The sale price.</param>
    /// <param name="saleStartDate">The sale start date.</param>
    /// <param name="saleEndDate">The sale end date.</param>
    /// <returns>A new pricing instance with sale information.</returns>
    public Pricing AddSale(decimal salePrice, DateTime? saleStartDate = null, DateTime? saleEndDate = null)
    {
        return CreateWithSale(RegularPrice, salePrice, saleStartDate, saleEndDate);
    }

    /// <summary>
    /// Removes sale information.
    /// </summary>
    /// <returns>A new pricing instance without sale information.</returns>
    public Pricing RemoveSale()
    {
        return new Pricing(RegularPrice, null, null, null);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return RegularPrice;
        yield return SalePrice ?? 0;
        yield return SaleStartDate ?? DateTime.MinValue;
        yield return SaleEndDate ?? DateTime.MinValue;
    }
}
