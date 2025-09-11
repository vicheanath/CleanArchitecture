using Shared.Primitives;

namespace Clean.Architecture.Domain.Products.ValueObjects;

/// <summary>
/// Represents a collection of product tags.
/// </summary>
public sealed class ProductTags : ValueObject
{
    private readonly HashSet<string> _tags;

    /// <summary>
    /// Gets the list of tags.
    /// </summary>
    public IReadOnlyCollection<string> Tags => _tags.ToList().AsReadOnly();

    /// <summary>
    /// Gets the number of tags.
    /// </summary>
    public int Count => _tags.Count;

    /// <summary>
    /// Gets a value indicating whether there are any tags.
    /// </summary>
    public bool HasTags => _tags.Any();

    private ProductTags(IEnumerable<string> tags)
    {
        _tags = new HashSet<string>(
            tags?.Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim().ToLowerInvariant()) ?? Enumerable.Empty<string>(),
            StringComparer.OrdinalIgnoreCase
        );
    }

    /// <summary>
    /// Creates a new product tags collection.
    /// </summary>
    /// <param name="tags">The tags.</param>
    /// <returns>A new product tags instance.</returns>
    public static ProductTags Create(IEnumerable<string>? tags = null)
    {
        var tagList = tags?.ToList() ?? new List<string>();

        // Validate tags
        foreach (var tag in tagList.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
            var trimmedTag = tag.Trim();

            if (trimmedTag.Length > 50)
                throw new ArgumentException($"Tag '{trimmedTag}' exceeds maximum length of 50 characters.", nameof(tags));

            if (trimmedTag.Contains(',') || trimmedTag.Contains(';'))
                throw new ArgumentException($"Tag '{trimmedTag}' cannot contain commas or semicolons.", nameof(tags));

            if (!IsValidTag(trimmedTag))
                throw new ArgumentException($"Tag '{trimmedTag}' contains invalid characters. Only letters, numbers, hyphens, and underscores are allowed.", nameof(tags));
        }

        var uniqueTags = tagList.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct(StringComparer.OrdinalIgnoreCase);

        if (uniqueTags.Count() > 20)
            throw new ArgumentException("Cannot have more than 20 tags per product.", nameof(tags));

        return new ProductTags(uniqueTags);
    }

    /// <summary>
    /// Creates an empty product tags collection.
    /// </summary>
    /// <returns>An empty product tags instance.</returns>
    public static ProductTags Empty => new(Enumerable.Empty<string>());

    /// <summary>
    /// Adds a tag to the collection.
    /// </summary>
    /// <param name="tag">The tag to add.</param>
    /// <returns>A new product tags instance with the added tag.</returns>
    public ProductTags AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Tag cannot be null or empty.", nameof(tag));

        var trimmedTag = tag.Trim();

        if (trimmedTag.Length > 50)
            throw new ArgumentException("Tag exceeds maximum length of 50 characters.", nameof(tag));

        if (!IsValidTag(trimmedTag))
            throw new ArgumentException("Tag contains invalid characters. Only letters, numbers, hyphens, and underscores are allowed.", nameof(tag));

        if (_tags.Contains(trimmedTag))
            return this; // Tag already exists

        if (_tags.Count >= 20)
            throw new InvalidOperationException("Cannot add more than 20 tags per product.");

        var newTags = new List<string>(_tags) { trimmedTag };
        return new ProductTags(newTags);
    }

    /// <summary>
    /// Adds multiple tags to the collection.
    /// </summary>
    /// <param name="tags">The tags to add.</param>
    /// <returns>A new product tags instance with the added tags.</returns>
    public ProductTags AddTags(IEnumerable<string> tags)
    {
        var result = this;
        foreach (var tag in tags ?? Enumerable.Empty<string>())
        {
            if (!string.IsNullOrWhiteSpace(tag))
                result = result.AddTag(tag);
        }
        return result;
    }

    /// <summary>
    /// Removes a tag from the collection.
    /// </summary>
    /// <param name="tag">The tag to remove.</param>
    /// <returns>A new product tags instance without the specified tag.</returns>
    public ProductTags RemoveTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Tag cannot be null or empty.", nameof(tag));

        var newTags = _tags.Where(t => !string.Equals(t, tag.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
        return new ProductTags(newTags);
    }

    /// <summary>
    /// Removes multiple tags from the collection.
    /// </summary>
    /// <param name="tags">The tags to remove.</param>
    /// <returns>A new product tags instance without the specified tags.</returns>
    public ProductTags RemoveTags(IEnumerable<string> tags)
    {
        var result = this;
        foreach (var tag in tags ?? Enumerable.Empty<string>())
        {
            if (!string.IsNullOrWhiteSpace(tag))
                result = result.RemoveTag(tag);
        }
        return result;
    }

    /// <summary>
    /// Checks if a tag exists in the collection.
    /// </summary>
    /// <param name="tag">The tag to check.</param>
    /// <returns>True if the tag exists, otherwise false.</returns>
    public bool HasTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return false;

        return _tags.Contains(tag.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Clears all tags.
    /// </summary>
    /// <returns>An empty product tags instance.</returns>
    public ProductTags Clear()
    {
        return Empty;
    }

    /// <summary>
    /// Gets tags that match a specific pattern or category.
    /// </summary>
    /// <param name="pattern">The pattern to match (case-insensitive).</param>
    /// <returns>Matching tags.</returns>
    public IEnumerable<string> GetTagsMatching(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return Enumerable.Empty<string>();

        return _tags.Where(tag => tag.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets tags formatted for display (e.g., proper case).
    /// </summary>
    /// <returns>Tags formatted for display.</returns>
    public IEnumerable<string> GetDisplayTags()
    {
        return _tags.Select(FormatTagForDisplay).OrderBy(tag => tag);
    }

    /// <summary>
    /// Gets tags as a comma-separated string.
    /// </summary>
    /// <returns>A comma-separated string of tags.</returns>
    public string GetTagsAsString()
    {
        return string.Join(", ", GetDisplayTags());
    }

    /// <summary>
    /// Gets popular tag categories based on common prefixes or patterns.
    /// </summary>
    /// <returns>A dictionary of categories and their tags.</returns>
    public Dictionary<string, List<string>> GetTagCategories()
    {
        var categories = new Dictionary<string, List<string>>();

        foreach (var tag in _tags)
        {
            var category = GetTagCategory(tag);
            if (!categories.ContainsKey(category))
                categories[category] = new List<string>();

            categories[category].Add(FormatTagForDisplay(tag));
        }

        return categories;
    }

    /// <summary>
    /// Validates if a tag contains only allowed characters.
    /// </summary>
    /// <param name="tag">The tag to validate.</param>
    /// <returns>True if the tag is valid, otherwise false.</returns>
    private static bool IsValidTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return false;

        return tag.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == ' ');
    }

    /// <summary>
    /// Formats a tag for display (proper case).
    /// </summary>
    /// <param name="tag">The tag to format.</param>
    /// <returns>The formatted tag.</returns>
    private static string FormatTagForDisplay(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return string.Empty;

        // Convert to proper case
        var words = tag.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        var formattedWords = words.Select(word =>
            char.ToUpperInvariant(word[0]) + (word.Length > 1 ? word.Substring(1).ToLowerInvariant() : ""));

        return string.Join(" ", formattedWords);
    }

    /// <summary>
    /// Determines the category of a tag based on common patterns.
    /// </summary>
    /// <param name="tag">The tag to categorize.</param>
    /// <returns>The category name.</returns>
    private static string GetTagCategory(string tag)
    {
        var lowerTag = tag.ToLowerInvariant();

        if (lowerTag.Contains("color") || lowerTag.Contains("red") || lowerTag.Contains("blue") || lowerTag.Contains("green") ||
            lowerTag.Contains("black") || lowerTag.Contains("white") || lowerTag.Contains("gray"))
            return "Colors";

        if (lowerTag.Contains("size") || lowerTag.Contains("small") || lowerTag.Contains("medium") || lowerTag.Contains("large") ||
            lowerTag.Contains("xl") || lowerTag.Contains("xs"))
            return "Sizes";

        if (lowerTag.Contains("material") || lowerTag.Contains("cotton") || lowerTag.Contains("leather") || lowerTag.Contains("metal") ||
            lowerTag.Contains("wood") || lowerTag.Contains("plastic"))
            return "Materials";

        if (lowerTag.Contains("brand") || lowerTag.Contains("premium") || lowerTag.Contains("luxury") || lowerTag.Contains("budget"))
            return "Brand";

        if (lowerTag.Contains("feature") || lowerTag.Contains("waterproof") || lowerTag.Contains("wireless") || lowerTag.Contains("smart"))
            return "Features";

        return "General";
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        return _tags.OrderBy(tag => tag).Cast<object>();
    }
}
