namespace Core.Models
{
    public sealed record CatalogProductDto(
        Guid Id,
        string Name,
        string? Description,
        decimal Price,
        int AvailableStock,
        string? CategoryName,
        string? ImageUrl,
        bool HasVariants = false,
        decimal? MinVariantPrice = null,
        decimal? MaxVariantPrice = null);
}
