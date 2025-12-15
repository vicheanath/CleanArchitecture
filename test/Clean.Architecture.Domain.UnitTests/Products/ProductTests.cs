using Clean.Architecture.Domain.Products;
using Clean.Architecture.Domain.Products.ValueObjects;

namespace Clean.Architecture.Domain.UnitTests.Products;

public class ProductTests
{
    [Fact]
    public void Create_WithValidParameters_CreatesProduct()
    {
        // Act
        var product = Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category");

        // Assert
        Assert.NotNull(product);
        Assert.Equal("SKU-001", product.Sku);
        Assert.Equal("Test Product", product.Name);
        Assert.Equal("Description", product.Description);
        Assert.Equal("Category", product.Category);
        Assert.Equal(10.00m, product.ProductPricing.RegularPrice);
        Assert.Equal(10.00m, product.EffectivePrice);
        Assert.False(product.IsOnSale);
        Assert.True(product.IsActive);
        Assert.NotNull(product.Id);
        Assert.NotEqual(default(DateTime), product.CreatedOnUtc);
        Assert.Null(product.ModifiedOnUtc);
    }

    [Fact]
    public void Create_WithEmptySku_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Product.Create("", "Test Product", "Description", 10.00m, "Category"));
    }

    [Fact]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Product.Create("SKU-001", "", "Description", 10.00m, "Category"));
    }

    [Fact]
    public void Create_WithZeroOrNegativePrice_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Product.Create("SKU-001", "Test Product", "Description", 0, "Category"));
        Assert.Throws<ArgumentException>(() => Product.Create("SKU-001", "Test Product", "Description", -10.00m, "Category"));
    }

    [Fact]
    public void Create_WithEmptyCategory_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Product.Create("SKU-001", "Test Product", "Description", 10.00m, ""));
    }

    [Fact]
    public void Create_WithSalePriceGreaterThanRegularPrice_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category", salePrice: 15.00m));
    }

    [Fact]
    public void Create_WithInvalidSaleDateRange_ThrowsArgumentException()
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddHours(-1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category", salePrice: 5.00m, saleStartDate: startDate, saleEndDate: endDate));
    }

    [Fact]
    public void Create_RaisesProductCreatedDomainEvent()
    {
        // Act
        var product = Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category");

        // Assert
        var domainEvents = product.GetDomainEvents();
        Assert.Single(domainEvents);
        var createdEvent = Assert.IsType<ProductCreatedDomainEvent>(domainEvents[0]);
        Assert.Equal(product.Id, createdEvent.ProductId);
        Assert.Equal("SKU-001", createdEvent.Sku);
        Assert.Equal("Test Product", createdEvent.Name);
        Assert.Equal("Category", createdEvent.Category);
        Assert.Equal(10.00m, createdEvent.Price);
    }

    [Fact]
    public void UpdatePrice_WithValidPrice_UpdatesPrice()
    {
        // Arrange
        var product = Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category");
        var oldPrice = product.ProductPricing.RegularPrice;
        product.ClearDomainEvents();

        // Act
        product.UpdatePrice(15.00m);

        // Assert
        Assert.Equal(15.00m, product.ProductPricing.RegularPrice);
        Assert.NotNull(product.ModifiedOnUtc);
        var domainEvents = product.GetDomainEvents();
        Assert.Single(domainEvents);
        var priceUpdatedEvent = Assert.IsType<ProductPriceUpdatedDomainEvent>(domainEvents[0]);
        Assert.Equal(oldPrice, priceUpdatedEvent.OldPrice);
        Assert.Equal(15.00m, priceUpdatedEvent.NewPrice);
    }

    [Fact]
    public void UpdatePrice_WithZeroOrNegativePrice_ThrowsArgumentException()
    {
        // Arrange
        var product = Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => product.UpdatePrice(0));
        Assert.Throws<ArgumentException>(() => product.UpdatePrice(-10.00m));
    }

    [Fact]
    public void UpdateInfo_WithValidParameters_UpdatesProductInfo()
    {
        // Arrange
        var product = Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category");
        product.ClearDomainEvents();

        // Act
        product.UpdateInfo("Updated Name", "Updated Description", "Updated Category", "Brand");

        // Assert
        Assert.Equal("Updated Name", product.Name);
        Assert.Equal("Updated Description", product.Description);
        Assert.Equal("Updated Category", product.Category);
        Assert.Equal("Brand", product.Brand);
        Assert.NotNull(product.ModifiedOnUtc);
        var domainEvents = product.GetDomainEvents();
        Assert.Single(domainEvents);
        Assert.IsType<ProductInfoUpdatedDomainEvent>(domainEvents[0]);
    }

    [Fact]
    public void UpdateInfo_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var product = Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => product.UpdateInfo("", "Description", "Category"));
    }

    [Fact]
    public void UpdateSaleInfo_WithValidParameters_UpdatesSaleInfo()
    {
        // Arrange
        var product = Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category");
        var saleStartDate = DateTime.UtcNow;
        var saleEndDate = saleStartDate.AddDays(7);
        product.ClearDomainEvents();

        // Act
        product.UpdateSaleInfo(5.00m, saleStartDate, saleEndDate);

        // Assert
        Assert.True(product.IsOnSale);
        Assert.Equal(5.00m, product.EffectivePrice);
        Assert.NotNull(product.ModifiedOnUtc);
        var domainEvents = product.GetDomainEvents();
        Assert.Single(domainEvents);
        Assert.IsType<ProductSaleUpdatedDomainEvent>(domainEvents[0]);
    }

    [Fact]
    public void UpdateSaleInfo_WithSalePriceGreaterThanRegularPrice_ThrowsArgumentException()
    {
        // Arrange
        var product = Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => product.UpdateSaleInfo(15.00m));
    }

    [Fact]
    public void AddImage_WithValidUrl_AddsImage()
    {
        // Arrange
        var product = Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category");
        var imageUrl = "https://example.com/image.jpg";

        // Act
        product.AddImage(imageUrl);

        // Assert
        Assert.Contains(imageUrl, product.Images.ImageUrls);
        Assert.NotNull(product.ModifiedOnUtc);
    }

    [Fact]
    public void AddImage_WithEmptyUrl_ThrowsArgumentException()
    {
        // Arrange
        var product = Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => product.AddImage(""));
    }

    [Fact]
    public void RemoveImage_WithValidUrl_RemovesImage()
    {
        // Arrange
        var product = Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category");
        var imageUrl = "https://example.com/image.jpg";
        product.AddImage(imageUrl);

        // Act
        product.RemoveImage(imageUrl);

        // Assert
        Assert.DoesNotContain(imageUrl, product.Images.ImageUrls);
        Assert.NotNull(product.ModifiedOnUtc);
    }

    [Fact]
    public void AddTag_WithValidTag_AddsTag()
    {
        // Arrange
        var product = Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category");

        // Act
        product.AddTag("electronics");

        // Assert
        Assert.True(product.Tags.HasTag("electronics"));
        Assert.NotNull(product.ModifiedOnUtc);
    }

    [Fact]
    public void AddTag_WithEmptyTag_ThrowsArgumentException()
    {
        // Arrange
        var product = Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => product.AddTag(""));
    }

    [Fact]
    public void RemoveTag_WithValidTag_RemovesTag()
    {
        // Arrange
        var product = Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category");
        product.AddTag("electronics");

        // Act
        product.RemoveTag("electronics");

        // Assert
        Assert.False(product.Tags.HasTag("electronics"));
        Assert.NotNull(product.ModifiedOnUtc);
    }

    [Fact]
    public void Activate_SetsIsActiveToTrue()
    {
        // Arrange
        var product = Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category");
        product.Deactivate();
        product.ClearDomainEvents();

        // Act
        product.Activate();

        // Assert
        Assert.True(product.IsActive);
        Assert.NotNull(product.ModifiedOnUtc);
        var domainEvents = product.GetDomainEvents();
        Assert.Single(domainEvents);
        Assert.IsType<ProductActivatedDomainEvent>(domainEvents[0]);
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        // Arrange
        var product = Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category");
        product.ClearDomainEvents();

        // Act
        product.Deactivate();

        // Assert
        Assert.False(product.IsActive);
        Assert.NotNull(product.ModifiedOnUtc);
        var domainEvents = product.GetDomainEvents();
        Assert.Single(domainEvents);
        Assert.IsType<ProductDeactivatedDomainEvent>(domainEvents[0]);
    }

    [Fact]
    public void EffectivePrice_WhenOnSale_ReturnsSalePrice()
    {
        // Arrange
        var product = Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category", salePrice: 5.00m, saleStartDate: DateTime.UtcNow.AddHours(-1), saleEndDate: DateTime.UtcNow.AddHours(1));

        // Assert
        Assert.True(product.IsOnSale);
        Assert.Equal(5.00m, product.EffectivePrice);
    }

    [Fact]
    public void EffectivePrice_WhenNotOnSale_ReturnsRegularPrice()
    {
        // Arrange
        var product = Product.Create("SKU-001", "Test Product", "Description", 10.00m, "Category");

        // Assert
        Assert.False(product.IsOnSale);
        Assert.Equal(10.00m, product.EffectivePrice);
    }
}
