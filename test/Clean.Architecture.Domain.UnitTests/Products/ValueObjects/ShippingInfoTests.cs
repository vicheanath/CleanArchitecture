using Clean.Architecture.Domain.Products.ValueObjects;

namespace Clean.Architecture.Domain.UnitTests.Products.ValueObjects;

public class ShippingInfoTests
{
    [Fact]
    public void CreatePhysical_WithValidWeight_CreatesPhysicalShippingInfo()
    {
        // Act
        var shippingInfo = ShippingInfo.CreatePhysical(5.5m);

        // Assert
        Assert.True(shippingInfo.RequiresShipping);
        Assert.Equal(5.5m, shippingInfo.ShippingWeight);
        Assert.False(shippingInfo.IsDigitalProduct);
        Assert.Equal(5.5m, shippingInfo.EffectiveShippingWeight);
    }

    [Fact]
    public void CreatePhysical_WithNegativeWeight_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ShippingInfo.CreatePhysical(-1m));
    }

    [Fact]
    public void CreatePhysical_WithWeightTooLarge_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ShippingInfo.CreatePhysical(10000m));
    }

    [Fact]
    public void CreateDigital_CreatesDigitalShippingInfo()
    {
        // Act
        var shippingInfo = ShippingInfo.CreateDigital();

        // Assert
        Assert.False(shippingInfo.RequiresShipping);
        Assert.Null(shippingInfo.ShippingWeight);
        Assert.True(shippingInfo.IsDigitalProduct);
        Assert.Equal(0, shippingInfo.EffectiveShippingWeight);
    }

    [Fact]
    public void Create_WithRequiresShippingTrue_CreatesPhysical()
    {
        // Act
        var shippingInfo = ShippingInfo.Create(true, 5.5m);

        // Assert
        Assert.True(shippingInfo.RequiresShipping);
        Assert.Equal(5.5m, shippingInfo.ShippingWeight);
    }

    [Fact]
    public void Create_WithRequiresShippingFalse_CreatesDigital()
    {
        // Act
        var shippingInfo = ShippingInfo.Create(false);

        // Assert
        Assert.False(shippingInfo.RequiresShipping);
        Assert.True(shippingInfo.IsDigitalProduct);
    }

    [Fact]
    public void WithShippingWeight_ForPhysicalProduct_UpdatesWeight()
    {
        // Arrange
        var shippingInfo = ShippingInfo.CreatePhysical(5.5m);

        // Act
        var updated = shippingInfo.WithShippingWeight(7.0m);

        // Assert
        Assert.Equal(7.0m, updated.ShippingWeight);
    }

    [Fact]
    public void WithShippingWeight_ForDigitalProduct_ThrowsInvalidOperationException()
    {
        // Arrange
        var shippingInfo = ShippingInfo.CreateDigital();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => shippingInfo.WithShippingWeight(5.5m));
    }

    [Fact]
    public void ConvertToDigital_ConvertsToDigital()
    {
        // Arrange
        var shippingInfo = ShippingInfo.CreatePhysical(5.5m);

        // Act
        var converted = shippingInfo.ConvertToDigital();

        // Assert
        Assert.False(converted.RequiresShipping);
        Assert.True(converted.IsDigitalProduct);
    }

    [Fact]
    public void ConvertToPhysical_ConvertsToPhysical()
    {
        // Arrange
        var shippingInfo = ShippingInfo.CreateDigital();

        // Act
        var converted = shippingInfo.ConvertToPhysical(5.5m);

        // Assert
        Assert.True(converted.RequiresShipping);
        Assert.Equal(5.5m, converted.ShippingWeight);
    }

    [Fact]
    public void GetShippingCategory_ForLightPackage_ReturnsLightPackage()
    {
        // Arrange
        var shippingInfo = ShippingInfo.CreatePhysical(0.5m);

        // Act
        var category = shippingInfo.GetShippingCategory();

        // Assert
        Assert.Equal("Light Package", category);
    }

    [Fact]
    public void GetShippingCategory_ForHeavyPackage_ReturnsHeavyPackage()
    {
        // Arrange
        var shippingInfo = ShippingInfo.CreatePhysical(15m); // Between 5 and 20 lbs

        // Act
        var category = shippingInfo.GetShippingCategory();

        // Assert
        Assert.Equal("Heavy Package", category);
    }

    [Fact]
    public void GetShippingCategory_ForBulkItem_ReturnsBulkItem()
    {
        // Arrange
        var shippingInfo = ShippingInfo.CreatePhysical(25m); // Between 20 and 50 lbs

        // Act
        var category = shippingInfo.GetShippingCategory();

        // Assert
        Assert.Equal("Bulk Item", category);
    }

    [Fact]
    public void GetShippingCategory_ForDigitalProduct_ReturnsDigital()
    {
        // Arrange
        var shippingInfo = ShippingInfo.CreateDigital();

        // Act
        var category = shippingInfo.GetShippingCategory();

        // Assert
        Assert.Equal("Digital", category);
    }

    [Fact]
    public void EstimateShippingCost_ForPhysicalProduct_ReturnsCost()
    {
        // Arrange
        var shippingInfo = ShippingInfo.CreatePhysical(5.5m);

        // Act
        var cost = shippingInfo.EstimateShippingCost(1.50m, 5.00m);

        // Assert
        Assert.True(cost > 0);
        Assert.True(cost >= 5.00m); // Minimum cost
    }

    [Fact]
    public void EstimateShippingCost_ForDigitalProduct_ReturnsZero()
    {
        // Arrange
        var shippingInfo = ShippingInfo.CreateDigital();

        // Act
        var cost = shippingInfo.EstimateShippingCost();

        // Assert
        Assert.Equal(0, cost);
    }

    [Fact]
    public void GetDisplayString_ForPhysicalProduct_ReturnsFormattedString()
    {
        // Arrange
        var shippingInfo = ShippingInfo.CreatePhysical(5.5m);

        // Act
        var displayString = shippingInfo.GetDisplayString();

        // Assert
        Assert.Contains("5.50", displayString);
        Assert.Contains("lbs", displayString);
    }

    [Fact]
    public void GetDisplayString_ForDigitalProduct_ReturnsDigitalMessage()
    {
        // Arrange
        var shippingInfo = ShippingInfo.CreateDigital();

        // Act
        var displayString = shippingInfo.GetDisplayString();

        // Assert
        Assert.Contains("Digital", displayString);
        Assert.Contains("No Shipping", displayString);
    }

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var shippingInfo1 = ShippingInfo.CreatePhysical(5.5m);
        var shippingInfo2 = ShippingInfo.CreatePhysical(5.5m);

        // Assert
        Assert.Equal(shippingInfo1, shippingInfo2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var shippingInfo1 = ShippingInfo.CreatePhysical(5.5m);
        var shippingInfo2 = ShippingInfo.CreatePhysical(7.0m);

        // Assert
        Assert.NotEqual(shippingInfo1, shippingInfo2);
    }
}
