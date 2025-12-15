using Clean.Architecture.Domain.Products.ValueObjects;

namespace Clean.Architecture.Domain.UnitTests.Products.ValueObjects;

public class PhysicalAttributesTests
{
    [Fact]
    public void Create_WithValidParameters_CreatesPhysicalAttributes()
    {
        // Act
        var attributes = PhysicalAttributes.Create(5.5m, "12x8x1", "Red", "Large");

        // Assert
        Assert.Equal(5.5m, attributes.Weight);
        Assert.Equal("12x8x1", attributes.Dimensions);
        Assert.Equal("Red", attributes.Color);
        Assert.Equal("Large", attributes.Size);
        Assert.True(attributes.HasPhysicalAttributes);
    }

    [Fact]
    public void Create_WithNegativeWeight_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => PhysicalAttributes.Create(-1m));
    }

    [Fact]
    public void Create_WithDimensionsTooLong_ThrowsArgumentException()
    {
        // Arrange
        var longDimensions = new string('a', 101);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => PhysicalAttributes.Create(dimensions: longDimensions));
    }

    [Fact]
    public void Create_WithColorTooLong_ThrowsArgumentException()
    {
        // Arrange
        var longColor = new string('a', 51);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => PhysicalAttributes.Create(color: longColor));
    }

    [Fact]
    public void Create_WithSizeTooLong_ThrowsArgumentException()
    {
        // Arrange
        var longSize = new string('a', 51);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => PhysicalAttributes.Create(size: longSize));
    }

    [Fact]
    public void HasPhysicalAttributes_WhenAllNull_ReturnsFalse()
    {
        // Act
        var attributes = PhysicalAttributes.Create();

        // Assert
        Assert.False(attributes.HasPhysicalAttributes);
    }

    [Fact]
    public void HasPhysicalAttributes_WhenAnyAttributeSet_ReturnsTrue()
    {
        // Act & Assert
        Assert.True(PhysicalAttributes.Create(weight: 5.5m).HasPhysicalAttributes);
        Assert.True(PhysicalAttributes.Create(dimensions: "12x8x1").HasPhysicalAttributes);
        Assert.True(PhysicalAttributes.Create(color: "Red").HasPhysicalAttributes);
        Assert.True(PhysicalAttributes.Create(size: "Large").HasPhysicalAttributes);
    }

    [Fact]
    public void WithWeight_UpdatesWeight()
    {
        // Arrange
        var attributes = PhysicalAttributes.Create(5.5m, "12x8x1", "Red", "Large");

        // Act
        var updated = attributes.WithWeight(7.0m);

        // Assert
        Assert.Equal(7.0m, updated.Weight);
        Assert.Equal("12x8x1", updated.Dimensions);
        Assert.Equal("Red", updated.Color);
        Assert.Equal("Large", updated.Size);
    }

    [Fact]
    public void WithDimensions_UpdatesDimensions()
    {
        // Arrange
        var attributes = PhysicalAttributes.Create(5.5m, "12x8x1", "Red", "Large");

        // Act
        var updated = attributes.WithDimensions("15x10x2");

        // Assert
        Assert.Equal(5.5m, updated.Weight);
        Assert.Equal("15x10x2", updated.Dimensions);
    }

    [Fact]
    public void WithColor_UpdatesColor()
    {
        // Arrange
        var attributes = PhysicalAttributes.Create(5.5m, "12x8x1", "Red", "Large");

        // Act
        var updated = attributes.WithColor("Blue");

        // Assert
        Assert.Equal("Blue", updated.Color);
    }

    [Fact]
    public void WithSize_UpdatesSize()
    {
        // Arrange
        var attributes = PhysicalAttributes.Create(5.5m, "12x8x1", "Red", "Large");

        // Act
        var updated = attributes.WithSize("Small");

        // Assert
        Assert.Equal("Small", updated.Size);
    }

    [Fact]
    public void GetDisplayString_ReturnsFormattedString()
    {
        // Arrange
        var attributes = PhysicalAttributes.Create(5.5m, "12x8x1", "Red", "Large");

        // Act
        var displayString = attributes.GetDisplayString();

        // Assert
        Assert.Contains("Color: Red", displayString);
        Assert.Contains("Size: Large", displayString);
        Assert.Contains("Weight: 5.50 lbs", displayString);
        Assert.Contains("Dimensions: 12x8x1", displayString);
    }

    [Fact]
    public void GetDisplayString_WithNoAttributes_ReturnsEmptyString()
    {
        // Arrange
        var attributes = PhysicalAttributes.Create();

        // Act
        var displayString = attributes.GetDisplayString();

        // Assert
        Assert.Empty(displayString);
    }

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var attributes1 = PhysicalAttributes.Create(5.5m, "12x8x1", "Red", "Large");
        var attributes2 = PhysicalAttributes.Create(5.5m, "12x8x1", "Red", "Large");

        // Assert
        Assert.Equal(attributes1, attributes2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var attributes1 = PhysicalAttributes.Create(5.5m, "12x8x1", "Red", "Large");
        var attributes2 = PhysicalAttributes.Create(6.0m, "12x8x1", "Red", "Large");

        // Assert
        Assert.NotEqual(attributes1, attributes2);
    }
}
