using Clean.Architecture.Domain.Products.ValueObjects;

namespace Clean.Architecture.Domain.UnitTests.Products.ValueObjects;

public class ProductTagsTests
{
    [Fact]
    public void Create_WithValidTags_CreatesProductTags()
    {
        // Arrange
        var tags = new List<string> { "electronics", "smartphone", "android" };

        // Act
        var productTags = ProductTags.Create(tags);

        // Assert
        Assert.Equal(3, productTags.Count);
        Assert.True(productTags.HasTags);
        Assert.True(productTags.HasTag("electronics"));
    }

    [Fact]
    public void Create_WithTagTooLong_ThrowsArgumentException()
    {
        // Arrange
        var longTag = new string('a', 51);
        var tags = new List<string> { longTag };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ProductTags.Create(tags));
    }

    [Fact]
    public void Create_WithTagContainingComma_ThrowsArgumentException()
    {
        // Arrange
        var tags = new List<string> { "electronics, smartphone" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ProductTags.Create(tags));
    }

    [Fact]
    public void Create_WithMoreThanTwentyTags_ThrowsArgumentException()
    {
        // Arrange
        var tags = Enumerable.Range(1, 21).Select(i => $"tag{i}").ToList();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ProductTags.Create(tags));
    }

    [Fact]
    public void Create_WithDuplicateTags_RemovesDuplicates()
    {
        // Arrange
        var tags = new List<string> { "electronics", "ELECTRONICS", "Electronics" };

        // Act
        var productTags = ProductTags.Create(tags);

        // Assert
        Assert.Equal(1, productTags.Count);
    }

    [Fact]
    public void AddTag_WithValidTag_AddsTag()
    {
        // Arrange
        var productTags = ProductTags.Create(new List<string> { "electronics" });

        // Act
        var updated = productTags.AddTag("smartphone");

        // Assert
        Assert.Equal(2, updated.Count);
        Assert.True(updated.HasTag("smartphone"));
    }

    [Fact]
    public void AddTag_WithDuplicateTag_ReturnsSameInstance()
    {
        // Arrange
        var productTags = ProductTags.Create(new List<string> { "electronics" });

        // Act
        var updated = productTags.AddTag("electronics");

        // Assert
        Assert.Equal(1, updated.Count);
    }

    [Fact]
    public void AddTag_WithMoreThanTwentyTags_ThrowsInvalidOperationException()
    {
        // Arrange
        var tags = Enumerable.Range(1, 20).Select(i => $"tag{i}").ToList();
        var productTags = ProductTags.Create(tags);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => productTags.AddTag("tag21"));
    }

    [Fact]
    public void RemoveTag_WithValidTag_RemovesTag()
    {
        // Arrange
        var productTags = ProductTags.Create(new List<string> { "electronics", "smartphone" });

        // Act
        var updated = productTags.RemoveTag("electronics");

        // Assert
        Assert.Equal(1, updated.Count);
        Assert.False(updated.HasTag("electronics"));
        Assert.True(updated.HasTag("smartphone"));
    }

    [Fact]
    public void RemoveTag_WithCaseInsensitiveMatch_RemovesTag()
    {
        // Arrange
        var productTags = ProductTags.Create(new List<string> { "electronics" });

        // Act
        var updated = productTags.RemoveTag("ELECTRONICS");

        // Assert
        Assert.Equal(0, updated.Count);
    }

    [Fact]
    public void AddTags_WithMultipleTags_AddsAllTags()
    {
        // Arrange
        var productTags = ProductTags.Create(new List<string> { "electronics" });

        // Act
        var updated = productTags.AddTags(new List<string> { "smartphone", "android" });

        // Assert
        Assert.Equal(3, updated.Count);
    }

    [Fact]
    public void RemoveTags_WithMultipleTags_RemovesAllTags()
    {
        // Arrange
        var productTags = ProductTags.Create(new List<string> { "electronics", "smartphone", "android" });

        // Act
        var updated = productTags.RemoveTags(new List<string> { "electronics", "smartphone" });

        // Assert
        Assert.Equal(1, updated.Count);
        Assert.True(updated.HasTag("android"));
    }

    [Fact]
    public void Clear_RemovesAllTags()
    {
        // Arrange
        var productTags = ProductTags.Create(new List<string> { "electronics", "smartphone" });

        // Act
        var cleared = productTags.Clear();

        // Assert
        Assert.Equal(0, cleared.Count);
        Assert.False(cleared.HasTags);
    }

    [Fact]
    public void GetTagsMatching_WithMatchingPattern_ReturnsMatchingTags()
    {
        // Arrange
        var productTags = ProductTags.Create(new List<string> { "electronics", "smartphone", "phone" });

        // Act
        var matching = productTags.GetTagsMatching("phone").ToList();

        // Assert
        Assert.Equal(2, matching.Count);
        Assert.Contains("smartphone", matching);
        Assert.Contains("phone", matching);
    }

    [Fact]
    public void GetDisplayTags_ReturnsFormattedTags()
    {
        // Arrange
        var productTags = ProductTags.Create(new List<string> { "electronics", "smartphone" });

        // Act
        var displayTags = productTags.GetDisplayTags().ToList();

        // Assert
        Assert.Equal(2, displayTags.Count);
        Assert.All(displayTags, tag => Assert.True(char.IsUpper(tag[0])));
    }

    [Fact]
    public void GetTagsAsString_ReturnsCommaSeparatedString()
    {
        // Arrange
        var productTags = ProductTags.Create(new List<string> { "electronics", "smartphone" });

        // Act
        var tagsString = productTags.GetTagsAsString();

        // Assert
        Assert.Contains("Electronics", tagsString);
        Assert.Contains("Smartphone", tagsString);
    }

    [Fact]
    public void GetTagCategories_ReturnsCategorizedTags()
    {
        // Arrange
        var productTags = ProductTags.Create(new List<string> { "red", "blue", "small", "large" });

        // Act
        var categories = productTags.GetTagCategories();

        // Assert
        Assert.True(categories.ContainsKey("Colors") || categories.ContainsKey("Sizes"));
    }
}
