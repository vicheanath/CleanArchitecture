using Shared.Primitives;

namespace Clean.Architecture.Domain.Products.ValueObjects;

/// <summary>
/// Represents shipping information for a product.
/// </summary>
public sealed class ShippingInfo : ValueObject
{
    /// <summary>
    /// Gets a value indicating whether the product requires shipping.
    /// </summary>
    public bool RequiresShipping { get; }

    /// <summary>
    /// Gets the shipping weight in pounds.
    /// </summary>
    public decimal? ShippingWeight { get; }

    /// <summary>
    /// Gets a value indicating whether the product is a digital product (doesn't require shipping).
    /// </summary>
    public bool IsDigitalProduct => !RequiresShipping;

    /// <summary>
    /// Gets the effective shipping weight (returns shipping weight or 0 for digital products).
    /// </summary>
    public decimal EffectiveShippingWeight => RequiresShipping ? (ShippingWeight ?? 0) : 0;

    private ShippingInfo(bool requiresShipping, decimal? shippingWeight)
    {
        RequiresShipping = requiresShipping;
        ShippingWeight = shippingWeight;
    }

    /// <summary>
    /// Creates shipping info for a physical product.
    /// </summary>
    /// <param name="shippingWeight">The shipping weight in pounds.</param>
    /// <returns>A new shipping info instance for a physical product.</returns>
    public static ShippingInfo CreatePhysical(decimal? shippingWeight = null)
    {
        if (shippingWeight.HasValue && shippingWeight.Value < 0)
            throw new ArgumentException("Shipping weight cannot be negative.", nameof(shippingWeight));

        if (shippingWeight.HasValue && shippingWeight.Value > 9999.99m)
            throw new ArgumentException("Shipping weight must be less than 10,000 pounds.", nameof(shippingWeight));

        return new ShippingInfo(true, shippingWeight);
    }

    /// <summary>
    /// Creates shipping info for a digital product.
    /// </summary>
    /// <returns>A new shipping info instance for a digital product.</returns>
    public static ShippingInfo CreateDigital()
    {
        return new ShippingInfo(false, null);
    }

    /// <summary>
    /// Creates shipping info based on requirements.
    /// </summary>
    /// <param name="requiresShipping">Whether the product requires shipping.</param>
    /// <param name="shippingWeight">The shipping weight (ignored for digital products).</param>
    /// <returns>A new shipping info instance.</returns>
    public static ShippingInfo Create(bool requiresShipping, decimal? shippingWeight = null)
    {
        return requiresShipping ? CreatePhysical(shippingWeight) : CreateDigital();
    }

    /// <summary>
    /// Updates the shipping weight for physical products.
    /// </summary>
    /// <param name="shippingWeight">The new shipping weight.</param>
    /// <returns>A new shipping info instance with updated weight.</returns>
    /// <exception cref="InvalidOperationException">Thrown when trying to set weight for digital products.</exception>
    public ShippingInfo WithShippingWeight(decimal? shippingWeight)
    {
        if (!RequiresShipping)
            throw new InvalidOperationException("Cannot set shipping weight for digital products.");

        return CreatePhysical(shippingWeight);
    }

    /// <summary>
    /// Converts to digital product (removes shipping requirement).
    /// </summary>
    /// <returns>A new shipping info instance for digital product.</returns>
    public ShippingInfo ConvertToDigital()
    {
        return CreateDigital();
    }

    /// <summary>
    /// Converts to physical product with optional shipping weight.
    /// </summary>
    /// <param name="shippingWeight">The shipping weight.</param>
    /// <returns>A new shipping info instance for physical product.</returns>
    public ShippingInfo ConvertToPhysical(decimal? shippingWeight = null)
    {
        return CreatePhysical(shippingWeight);
    }

    /// <summary>
    /// Gets shipping category based on weight.
    /// </summary>
    /// <returns>The shipping category.</returns>
    public string GetShippingCategory()
    {
        if (!RequiresShipping)
            return "Digital";

        var weight = EffectiveShippingWeight;

        return weight switch
        {
            0 => "No Weight",
            <= 1 => "Light Package",
            <= 5 => "Standard Package",
            <= 20 => "Heavy Package",
            <= 50 => "Bulk Item",
            _ => "Freight"
        };
    }

    /// <summary>
    /// Estimates shipping cost based on weight (simplified calculation).
    /// </summary>
    /// <param name="baseRate">The base shipping rate per pound.</param>
    /// <param name="minimumCost">The minimum shipping cost.</param>
    /// <returns>The estimated shipping cost.</returns>
    public decimal EstimateShippingCost(decimal baseRate = 1.50m, decimal minimumCost = 5.00m)
    {
        if (!RequiresShipping)
            return 0;

        var weight = EffectiveShippingWeight;
        if (weight == 0)
            return minimumCost;

        var cost = weight * baseRate;
        return Math.Max(cost, minimumCost);
    }

    /// <summary>
    /// Gets a display string for shipping information.
    /// </summary>
    /// <returns>A formatted string describing shipping info.</returns>
    public string GetDisplayString()
    {
        if (!RequiresShipping)
            return "Digital Product - No Shipping Required";

        var weight = EffectiveShippingWeight;
        var category = GetShippingCategory();

        return weight > 0
            ? $"Ships at {weight:F2} lbs ({category})"
            : $"Physical Product ({category})";
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return RequiresShipping;
        yield return ShippingWeight ?? 0;
    }
}
