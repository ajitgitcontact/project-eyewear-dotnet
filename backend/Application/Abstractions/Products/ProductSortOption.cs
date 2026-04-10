namespace backend.Application.Abstractions.Products;

/// <summary>
/// Defines available sorting options for product listing
/// </summary>
public static class ProductSortOption
{
    /// <summary>Sort by base price ascending (low to high)</summary>
    public const string PriceAscending = "price_asc";

    /// <summary>Sort by base price descending (high to low)</summary>
    public const string PriceDescending = "price_desc";

    /// <summary>Sort by sold quantity descending (popularity)</summary>
    public const string Popularity = "popularity";

    /// <summary>Sort by created date descending (newest first)</summary>
    public const string Newest = "newest";

    /// <summary>Sort by priority column (default sort)</summary>
    public const string Default = "default";

    /// <summary>Get all valid sort options</summary>
    public static readonly HashSet<string> ValidOptions = new()
    {
        PriceAscending,
        PriceDescending,
        Popularity,
        Newest,
        Default
    };

    /// <summary>Check if the provided sort option is valid</summary>
    public static bool IsValid(string? sortOption)
    {
        return string.IsNullOrWhiteSpace(sortOption) || ValidOptions.Contains(sortOption.ToLowerInvariant());
    }

    /// <summary>Normalize sort option to lowercase, default to Default if empty</summary>
    public static string Normalize(string? sortOption)
    {
        return string.IsNullOrWhiteSpace(sortOption) ? Default : sortOption.ToLowerInvariant();
    }
}
