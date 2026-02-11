using Core.Entities;

namespace Core.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Product>> ListAsync();
        Task<IReadOnlyList<Product>> ListByCategoryAsync(Guid? categoryId, string? search);
        Task<(IReadOnlyList<Product> Products, int TotalCount)> SearchAsync(string? search, Guid? categoryId, decimal? minPrice, decimal? maxPrice, int? minRating, bool? inStock, string? sortBy, int page, int pageSize);
        Task<IReadOnlyList<Product>> GetFeaturedAsync(int count);
        Task<IReadOnlyList<Product>> ListDeletedAsync();
        Task AddAsync(Product product);
        Task AddImagesAsync(Guid productId, IReadOnlyList<ProductImage> images);
        Task<ProductImage?> GetImageByIdAsync(Guid imageId);
        Task RemoveImageAsync(Guid imageId);
        Task MoveImageAsync(Guid imageId, int direction);
        Task UpdateImageOrderAsync(Guid productId, IReadOnlyDictionary<Guid, int> orderMap);
        Task AddSpecificationAsync(Guid productId, ProductSpecification specification);
        Task RemoveSpecificationAsync(Guid specificationId);
        Task AddReviewAsync(Guid productId, ProductReview review);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Guid id);
        Task RestoreAsync(Guid id);
    }
}