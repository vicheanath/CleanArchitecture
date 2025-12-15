using Clean.Architecture.Domain.Products.ValueObjects;

namespace Clean.Architecture.Domain.UnitTests.Products.ValueObjects;

public class PricingTests
{
    [Fact]
    public void Create_WithValidPrice_CreatesPricing()
    {
        // Act
        var pricing = Pricing.Create(10.00m);

        // Assert
        Assert.Equal(10.00m, pricing.RegularPrice);
        Assert.Null(pricing.SalePrice);
        Assert.False(pricing.IsOnSale);
        Assert.Equal(10.00m, pricing.EffectivePrice);
    }

    [Fact]
    public void Create_WithZeroOrNegativePrice_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Pricing.Create(0));
        Assert.Throws<ArgumentException>(() => Pricing.Create(-10.00m));
    }

    [Fact]
    public void CreateWithSale_WithValidParameters_CreatesPricingWithSale()
    {
        // Arrange
        var saleStartDate = DateTime.UtcNow.AddHours(-1);
        var saleEndDate = DateTime.UtcNow.AddHours(1);

        // Act
        var pricing = Pricing.CreateWithSale(10.00m, 5.00m, saleStartDate, saleEndDate);

        // Assert
        Assert.Equal(10.00m, pricing.RegularPrice);
        Assert.Equal(5.00m, pricing.SalePrice);
        Assert.True(pricing.IsOnSale);
        Assert.Equal(5.00m, pricing.EffectivePrice);
    }

    [Fact]
    public void CreateWithSale_WithSalePriceGreaterThanRegularPrice_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Pricing.CreateWithSale(10.00m, 15.00m));
    }

    [Fact]
    public void CreateWithSale_WithInvalidDateRange_ThrowsArgumentException()
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddHours(-1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Pricing.CreateWithSale(10.00m, 5.00m, startDate, endDate));
    }

    [Fact]
    public void IsOnSale_WhenSaleHasNotStarted_ReturnsFalse()
    {
        // Arrange
        var saleStartDate = DateTime.UtcNow.AddHours(1);
        var saleEndDate = DateTime.UtcNow.AddHours(2);

        // Act
        var pricing = Pricing.CreateWithSale(10.00m, 5.00m, saleStartDate, saleEndDate);

        // Assert
        Assert.False(pricing.IsOnSale);
        Assert.Equal(10.00m, pricing.EffectivePrice);
    }

    [Fact]
    public void IsOnSale_WhenSaleHasEnded_ReturnsFalse()
    {
        // Arrange
        var saleStartDate = DateTime.UtcNow.AddHours(-2);
        var saleEndDate = DateTime.UtcNow.AddHours(-1);

        // Act
        var pricing = Pricing.CreateWithSale(10.00m, 5.00m, saleStartDate, saleEndDate);

        // Assert
        Assert.False(pricing.IsOnSale);
        Assert.Equal(10.00m, pricing.EffectivePrice);
    }

    [Fact]
    public void IsOnSale_WhenNoDatesSpecified_ReturnsTrue()
    {
        // Act
        var pricing = Pricing.CreateWithSale(10.00m, 5.00m);

        // Assert
        Assert.True(pricing.IsOnSale);
        Assert.Equal(5.00m, pricing.EffectivePrice);
    }

    [Fact]
    public void DiscountPercentage_WhenOnSale_ReturnsCorrectPercentage()
    {
        // Arrange
        var pricing = Pricing.CreateWithSale(10.00m, 5.00m);

        // Assert
        Assert.Equal(50.00m, pricing.DiscountPercentage);
    }

    [Fact]
    public void DiscountPercentage_WhenNotOnSale_ReturnsNull()
    {
        // Arrange
        var pricing = Pricing.Create(10.00m);

        // Assert
        Assert.Null(pricing.DiscountPercentage);
    }

    [Fact]
    public void UpdateRegularPrice_WithValidPrice_UpdatesPrice()
    {
        // Arrange
        var pricing = Pricing.Create(10.00m);

        // Act
        var updatedPricing = pricing.UpdateRegularPrice(15.00m);

        // Assert
        Assert.Equal(15.00m, updatedPricing.RegularPrice);
        Assert.Equal(15.00m, updatedPricing.EffectivePrice);
    }

    [Fact]
    public void UpdateRegularPrice_WithPriceLessThanSalePrice_ThrowsArgumentException()
    {
        // Arrange
        var pricing = Pricing.CreateWithSale(10.00m, 5.00m);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => pricing.UpdateRegularPrice(3.00m));
    }

    [Fact]
    public void AddSale_WithValidParameters_AddsSale()
    {
        // Arrange
        var pricing = Pricing.Create(10.00m);

        // Act
        var pricingWithSale = pricing.AddSale(5.00m);

        // Assert
        Assert.True(pricingWithSale.IsOnSale);
        Assert.Equal(5.00m, pricingWithSale.SalePrice);
    }

    [Fact]
    public void RemoveSale_RemovesSaleInformation()
    {
        // Arrange
        var pricing = Pricing.CreateWithSale(10.00m, 5.00m);

        // Act
        var pricingWithoutSale = pricing.RemoveSale();

        // Assert
        Assert.False(pricingWithoutSale.IsOnSale);
        Assert.Null(pricingWithoutSale.SalePrice);
        Assert.Equal(10.00m, pricingWithoutSale.EffectivePrice);
    }

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var pricing1 = Pricing.Create(10.00m);
        var pricing2 = Pricing.Create(10.00m);

        // Assert
        Assert.Equal(pricing1, pricing2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var pricing1 = Pricing.Create(10.00m);
        var pricing2 = Pricing.Create(15.00m);

        // Assert
        Assert.NotEqual(pricing1, pricing2);
    }
}
