using Shared.Primitives;

namespace Clean.Architecture.Domain.Products.ValueObjects;

/// <summary>
/// Represents a collection of product images.
/// </summary>
public sealed class ProductImages : ValueObject
{
    private readonly List<string> _imageUrls;

    /// <summary>
    /// Gets the list of image URLs.
    /// </summary>
    public IReadOnlyList<string> ImageUrls => _imageUrls.AsReadOnly();

    /// <summary>
    /// Gets the primary image URL (first image in the collection).
    /// </summary>
    public string? PrimaryImageUrl => _imageUrls.FirstOrDefault();

    /// <summary>
    /// Gets the number of images.
    /// </summary>
    public int Count => _imageUrls.Count;

    /// <summary>
    /// Gets a value indicating whether there are any images.
    /// </summary>
    public bool HasImages => _imageUrls.Any();

    /// <summary>
    /// Gets a value indicating whether there are multiple images.
    /// </summary>
    public bool HasMultipleImages => _imageUrls.Count > 1;

    private ProductImages(IEnumerable<string> imageUrls)
    {
        _imageUrls = imageUrls?.Where(url => !string.IsNullOrWhiteSpace(url)).ToList() ?? new List<string>();
    }

    /// <summary>
    /// Creates a new product images collection.
    /// </summary>
    /// <param name="imageUrls">The image URLs.</param>
    /// <returns>A new product images instance.</returns>
    public static ProductImages Create(IEnumerable<string>? imageUrls = null)
    {
        var urls = imageUrls?.ToList() ?? new List<string>();

        // Validate URLs
        foreach (var url in urls.Where(u => !string.IsNullOrWhiteSpace(u)))
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
                throw new ArgumentException($"Invalid image URL: {url}", nameof(imageUrls));
        }

        if (urls.Count > 10)
            throw new ArgumentException("Cannot have more than 10 images per product.", nameof(imageUrls));

        return new ProductImages(urls);
    }

    /// <summary>
    /// Creates an empty product images collection.
    /// </summary>
    /// <returns>An empty product images instance.</returns>
    public static ProductImages Empty => new(Enumerable.Empty<string>());

    /// <summary>
    /// Adds an image URL to the collection.
    /// </summary>
    /// <param name="imageUrl">The image URL to add.</param>
    /// <returns>A new product images instance with the added image.</returns>
    public ProductImages AddImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL cannot be null or empty.", nameof(imageUrl));

        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
            throw new ArgumentException($"Invalid image URL: {imageUrl}", nameof(imageUrl));

        if (_imageUrls.Contains(imageUrl))
            throw new ArgumentException("Image URL already exists in the collection.", nameof(imageUrl));

        if (_imageUrls.Count >= 10)
            throw new InvalidOperationException("Cannot add more than 10 images per product.");

        var newUrls = new List<string>(_imageUrls) { imageUrl };
        return new ProductImages(newUrls);
    }

    /// <summary>
    /// Removes an image URL from the collection.
    /// </summary>
    /// <param name="imageUrl">The image URL to remove.</param>
    /// <returns>A new product images instance without the specified image.</returns>
    public ProductImages RemoveImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL cannot be null or empty.", nameof(imageUrl));

        var newUrls = _imageUrls.Where(url => url != imageUrl).ToList();
        return new ProductImages(newUrls);
    }

    /// <summary>
    /// Removes an image at the specified index.
    /// </summary>
    /// <param name="index">The index of the image to remove.</param>
    /// <returns>A new product images instance without the image at the specified index.</returns>
    public ProductImages RemoveImageAt(int index)
    {
        if (index < 0 || index >= _imageUrls.Count)
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");

        var newUrls = _imageUrls.Where((_, i) => i != index).ToList();
        return new ProductImages(newUrls);
    }

    /// <summary>
    /// Sets the primary image by moving it to the first position.
    /// </summary>
    /// <param name="imageUrl">The image URL to set as primary.</param>
    /// <returns>A new product images instance with the specified image as primary.</returns>
    public ProductImages SetPrimaryImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL cannot be null or empty.", nameof(imageUrl));

        if (!_imageUrls.Contains(imageUrl))
            throw new ArgumentException("Image URL does not exist in the collection.", nameof(imageUrl));

        var newUrls = new List<string> { imageUrl };
        newUrls.AddRange(_imageUrls.Where(url => url != imageUrl));

        return new ProductImages(newUrls);
    }

    /// <summary>
    /// Reorders images by moving an image from one position to another.
    /// </summary>
    /// <param name="fromIndex">The current index of the image.</param>
    /// <param name="toIndex">The target index for the image.</param>
    /// <returns>A new product images instance with reordered images.</returns>
    public ProductImages ReorderImage(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= _imageUrls.Count)
            throw new ArgumentOutOfRangeException(nameof(fromIndex), "From index is out of range.");

        if (toIndex < 0 || toIndex >= _imageUrls.Count)
            throw new ArgumentOutOfRangeException(nameof(toIndex), "To index is out of range.");

        if (fromIndex == toIndex)
            return this;

        var newUrls = new List<string>(_imageUrls);
        var imageUrl = newUrls[fromIndex];
        newUrls.RemoveAt(fromIndex);
        newUrls.Insert(toIndex, imageUrl);

        return new ProductImages(newUrls);
    }

    /// <summary>
    /// Clears all images.
    /// </summary>
    /// <returns>An empty product images instance.</returns>
    public ProductImages Clear()
    {
        return Empty;
    }

    /// <summary>
    /// Gets images for a specific use case (e.g., thumbnail, gallery).
    /// </summary>
    /// <param name="maxCount">The maximum number of images to return.</param>
    /// <returns>A subset of images.</returns>
    public IEnumerable<string> GetImagesForDisplay(int maxCount = int.MaxValue)
    {
        return _imageUrls.Take(Math.Max(1, maxCount));
    }

    /// <summary>
    /// Gets thumbnail images (typically the first 3 images).
    /// </summary>
    /// <returns>Thumbnail images.</returns>
    public IEnumerable<string> GetThumbnailImages()
    {
        return GetImagesForDisplay(3);
    }

    /// <summary>
    /// Validates all image URLs by attempting to create Uri objects.
    /// </summary>
    /// <returns>A tuple indicating whether all URLs are valid and any invalid URLs.</returns>
    public (bool AllValid, IEnumerable<string> InvalidUrls) ValidateUrls()
    {
        var invalidUrls = new List<string>();

        foreach (var url in _imageUrls)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
            {
                invalidUrls.Add(url);
            }
        }

        return (invalidUrls.Count == 0, invalidUrls);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        return _imageUrls.Cast<object>();
    }
}
