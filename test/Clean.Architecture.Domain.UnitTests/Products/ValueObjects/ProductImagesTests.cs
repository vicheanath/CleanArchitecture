using Clean.Architecture.Domain.Products.ValueObjects;

namespace Clean.Architecture.Domain.UnitTests.Products.ValueObjects;

public class ProductImagesTests
{
    [Fact]
    public void Create_WithValidUrls_CreatesProductImages()
    {
        // Arrange
        var imageUrls = new List<string>
        {
            "https://example.com/image1.jpg",
            "https://example.com/image2.jpg"
        };

        // Act
        var images = ProductImages.Create(imageUrls);

        // Assert
        Assert.Equal(2, images.Count);
        Assert.True(images.HasImages);
        Assert.True(images.HasMultipleImages);
        Assert.Equal("https://example.com/image1.jpg", images.PrimaryImageUrl);
    }

    [Fact]
    public void Create_WithInvalidUrl_ThrowsArgumentException()
    {
        // Arrange
        var imageUrls = new List<string> { "not-a-valid-url" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ProductImages.Create(imageUrls));
    }

    [Fact]
    public void Create_WithMoreThanTenImages_ThrowsArgumentException()
    {
        // Arrange
        var imageUrls = Enumerable.Range(1, 11)
            .Select(i => $"https://example.com/image{i}.jpg")
            .ToList();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ProductImages.Create(imageUrls));
    }

    [Fact]
    public void Create_WithEmptyList_CreatesEmptyImages()
    {
        // Act
        var images = ProductImages.Create(new List<string>());

        // Assert
        Assert.Equal(0, images.Count);
        Assert.False(images.HasImages);
        Assert.Null(images.PrimaryImageUrl);
    }

    [Fact]
    public void AddImage_WithValidUrl_AddsImage()
    {
        // Arrange
        var images = ProductImages.Create(new List<string> { "https://example.com/image1.jpg" });

        // Act
        var updated = images.AddImage("https://example.com/image2.jpg");

        // Assert
        Assert.Equal(2, updated.Count);
        Assert.Contains("https://example.com/image2.jpg", updated.ImageUrls);
    }

    [Fact]
    public void AddImage_WithDuplicateUrl_ThrowsArgumentException()
    {
        // Arrange
        var images = ProductImages.Create(new List<string> { "https://example.com/image1.jpg" });

        // Act & Assert
        Assert.Throws<ArgumentException>(() => images.AddImage("https://example.com/image1.jpg"));
    }

    [Fact]
    public void AddImage_WithMoreThanTenImages_ThrowsInvalidOperationException()
    {
        // Arrange
        var imageUrls = Enumerable.Range(1, 10)
            .Select(i => $"https://example.com/image{i}.jpg")
            .ToList();
        var images = ProductImages.Create(imageUrls);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => images.AddImage("https://example.com/image11.jpg"));
    }

    [Fact]
    public void RemoveImage_WithValidUrl_RemovesImage()
    {
        // Arrange
        var images = ProductImages.Create(new List<string>
        {
            "https://example.com/image1.jpg",
            "https://example.com/image2.jpg"
        });

        // Act
        var updated = images.RemoveImage("https://example.com/image1.jpg");

        // Assert
        Assert.Equal(1, updated.Count);
        Assert.DoesNotContain("https://example.com/image1.jpg", updated.ImageUrls);
    }

    [Fact]
    public void RemoveImageAt_WithValidIndex_RemovesImage()
    {
        // Arrange
        var images = ProductImages.Create(new List<string>
        {
            "https://example.com/image1.jpg",
            "https://example.com/image2.jpg"
        });

        // Act
        var updated = images.RemoveImageAt(0);

        // Assert
        Assert.Equal(1, updated.Count);
        Assert.Equal("https://example.com/image2.jpg", updated.PrimaryImageUrl);
    }

    [Fact]
    public void RemoveImageAt_WithInvalidIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var images = ProductImages.Create(new List<string> { "https://example.com/image1.jpg" });

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => images.RemoveImageAt(1));
    }

    [Fact]
    public void SetPrimaryImage_MovesImageToFirstPosition()
    {
        // Arrange
        var images = ProductImages.Create(new List<string>
        {
            "https://example.com/image1.jpg",
            "https://example.com/image2.jpg",
            "https://example.com/image3.jpg"
        });

        // Act
        var updated = images.SetPrimaryImage("https://example.com/image3.jpg");

        // Assert
        Assert.Equal("https://example.com/image3.jpg", updated.PrimaryImageUrl);
        Assert.Equal(3, updated.Count);
    }

    [Fact]
    public void SetPrimaryImage_WithNonExistentUrl_ThrowsArgumentException()
    {
        // Arrange
        var images = ProductImages.Create(new List<string> { "https://example.com/image1.jpg" });

        // Act & Assert
        Assert.Throws<ArgumentException>(() => images.SetPrimaryImage("https://example.com/nonexistent.jpg"));
    }

    [Fact]
    public void ReorderImage_MovesImageToNewPosition()
    {
        // Arrange
        var images = ProductImages.Create(new List<string>
        {
            "https://example.com/image1.jpg",
            "https://example.com/image2.jpg",
            "https://example.com/image3.jpg"
        });

        // Act
        var updated = images.ReorderImage(0, 2);

        // Assert
        Assert.Equal("https://example.com/image2.jpg", updated.ImageUrls[0]);
        Assert.Equal("https://example.com/image3.jpg", updated.ImageUrls[1]);
        Assert.Equal("https://example.com/image1.jpg", updated.ImageUrls[2]);
    }

    [Fact]
    public void Clear_RemovesAllImages()
    {
        // Arrange
        var images = ProductImages.Create(new List<string>
        {
            "https://example.com/image1.jpg",
            "https://example.com/image2.jpg"
        });

        // Act
        var cleared = images.Clear();

        // Assert
        Assert.Equal(0, cleared.Count);
        Assert.False(cleared.HasImages);
    }

    [Fact]
    public void GetThumbnailImages_ReturnsFirstThreeImages()
    {
        // Arrange
        var images = ProductImages.Create(new List<string>
        {
            "https://example.com/image1.jpg",
            "https://example.com/image2.jpg",
            "https://example.com/image3.jpg",
            "https://example.com/image4.jpg"
        });

        // Act
        var thumbnails = images.GetThumbnailImages().ToList();

        // Assert
        Assert.Equal(3, thumbnails.Count);
        Assert.Equal("https://example.com/image1.jpg", thumbnails[0]);
    }

    [Fact]
    public void ValidateUrls_WithValidUrls_ReturnsAllValid()
    {
        // Arrange
        var images = ProductImages.Create(new List<string>
        {
            "https://example.com/image1.jpg",
            "https://example.com/image2.jpg"
        });

        // Act
        var (allValid, invalidUrls) = images.ValidateUrls();

        // Assert
        Assert.True(allValid);
        Assert.Empty(invalidUrls);
    }
}
