using Shared.Primitives;

namespace Clean.Architecture.Domain.Products.ValueObjects;

/// <summary>
/// Represents the physical attributes of a product.
/// </summary>
public sealed class PhysicalAttributes : ValueObject
{
    /// <summary>
    /// Gets the weight of the product in pounds.
    /// </summary>
    public decimal? Weight { get; }

    /// <summary>
    /// Gets the dimensions of the product (e.g., "12 x 8 x 1 inches").
    /// </summary>
    public string? Dimensions { get; }

    /// <summary>
    /// Gets the color of the product.
    /// </summary>
    public string? Color { get; }

    /// <summary>
    /// Gets the size of the product.
    /// </summary>
    public string? Size { get; }

    /// <summary>
    /// Gets a value indicating whether the product has physical attributes defined.
    /// </summary>
    public bool HasPhysicalAttributes => Weight.HasValue || !string.IsNullOrWhiteSpace(Dimensions) ||
                                        !string.IsNullOrWhiteSpace(Color) || !string.IsNullOrWhiteSpace(Size);

    private PhysicalAttributes(decimal? weight, string? dimensions, string? color, string? size)
    {
        Weight = weight;
        Dimensions = dimensions?.Trim();
        Color = color?.Trim();
        Size = size?.Trim();
    }

    /// <summary>
    /// Creates a new physical attributes instance.
    /// </summary>
    /// <param name="weight">The weight in pounds.</param>
    /// <param name="dimensions">The dimensions.</param>
    /// <param name="color">The color.</param>
    /// <param name="size">The size.</param>
    /// <returns>A new physical attributes instance.</returns>
    public static PhysicalAttributes Create(decimal? weight = null, string? dimensions = null, string? color = null, string? size = null)
    {
        if (weight.HasValue && weight.Value < 0)
            throw new ArgumentException("Weight cannot be negative.", nameof(weight));

        if (!string.IsNullOrWhiteSpace(dimensions) && dimensions.Length > 100)
            throw new ArgumentException("Dimensions must be less than 100 characters.", nameof(dimensions));

        if (!string.IsNullOrWhiteSpace(color) && color.Length > 50)
            throw new ArgumentException("Color must be less than 50 characters.", nameof(color));

        if (!string.IsNullOrWhiteSpace(size) && size.Length > 50)
            throw new ArgumentException("Size must be less than 50 characters.", nameof(size));

        return new PhysicalAttributes(weight, dimensions, color, size);
    }

    /// <summary>
    /// Creates an empty physical attributes instance.
    /// </summary>
    /// <returns>An empty physical attributes instance.</returns>
    public static PhysicalAttributes Empty => new(null, null, null, null);

    /// <summary>
    /// Updates the weight.
    /// </summary>
    /// <param name="weight">The new weight.</param>
    /// <returns>A new physical attributes instance with updated weight.</returns>
    public PhysicalAttributes WithWeight(decimal? weight)
    {
        return Create(weight, Dimensions, Color, Size);
    }

    /// <summary>
    /// Updates the dimensions.
    /// </summary>
    /// <param name="dimensions">The new dimensions.</param>
    /// <returns>A new physical attributes instance with updated dimensions.</returns>
    public PhysicalAttributes WithDimensions(string? dimensions)
    {
        return Create(Weight, dimensions, Color, Size);
    }

    /// <summary>
    /// Updates the color.
    /// </summary>
    /// <param name="color">The new color.</param>
    /// <returns>A new physical attributes instance with updated color.</returns>
    public PhysicalAttributes WithColor(string? color)
    {
        return Create(Weight, Dimensions, color, Size);
    }

    /// <summary>
    /// Updates the size.
    /// </summary>
    /// <param name="size">The new size.</param>
    /// <returns>A new physical attributes instance with updated size.</returns>
    public PhysicalAttributes WithSize(string? size)
    {
        return Create(Weight, Dimensions, Color, size);
    }

    /// <summary>
    /// Gets a formatted string representation of the physical attributes.
    /// </summary>
    /// <returns>A formatted string.</returns>
    public string GetDisplayString()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(Color))
            parts.Add($"Color: {Color}");

        if (!string.IsNullOrWhiteSpace(Size))
            parts.Add($"Size: {Size}");

        if (Weight.HasValue)
            parts.Add($"Weight: {Weight:F2} lbs");

        if (!string.IsNullOrWhiteSpace(Dimensions))
            parts.Add($"Dimensions: {Dimensions}");

        return string.Join(", ", parts);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Weight ?? 0;
        yield return Dimensions ?? string.Empty;
        yield return Color ?? string.Empty;
        yield return Size ?? string.Empty;
    }
}
