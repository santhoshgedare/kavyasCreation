using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _db;
        public ProductRepository(AppDbContext db) => _db = db;

        public async Task<Product?> GetByIdAsync(Guid id) => await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.Specifications)
            .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IReadOnlyList<Product>> ListAsync() => await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.Specifications)
            .AsNoTracking()
            .ToListAsync();

        public async Task<IReadOnlyList<Product>> ListByCategoryAsync(Guid? categoryId, string? search)
        {
            var query = _db.Products.Include(p => p.Category)
                .Include(p => p.Images.OrderBy(i => i.SortOrder))
                .Include(p => p.Specifications)
                .AsNoTracking()
                .AsQueryable();

            if (categoryId.HasValue && categoryId != Guid.Empty)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = $"%{search}%";
                query = query.Where(p => EF.Functions.Like(p.Name, term) || (p.Description != null && EF.Functions.Like(p.Description, term)));
            }

            return await query.ToListAsync();
        }

        public async Task<(IReadOnlyList<Product> Products, int TotalCount)> SearchAsync(string? search, Guid? categoryId, decimal? minPrice, decimal? maxPrice, int? minRating, bool? inStock, string? sortBy, int page, int pageSize)
        {
            var query = _db.Products
                .Include(p => p.Category)
                .Include(p => p.Images.OrderBy(i => i.SortOrder))
                .Include(p => p.Specifications)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = $"%{search}%";
                query = query.Where(p => EF.Functions.Like(p.Name, term) || (p.Description != null && EF.Functions.Like(p.Description, term)));
            }

            if (categoryId.HasValue && categoryId != Guid.Empty)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (minRating.HasValue)
            {
                query = query.Where(p => p.AverageRating >= minRating.Value);
            }

            if (inStock.HasValue && inStock.Value)
            {
                query = query.Where(p => p.Stock > 0);
            }

            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "rating" => query.OrderByDescending(p => p.AverageRating),
                "newest" => query.OrderByDescending(p => p.Id),
                _ => query.OrderBy(p => p.Name)
            };

            var totalCount = await query.CountAsync();
            var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (products, totalCount);
        }

        public async Task<IReadOnlyList<Product>> GetFeaturedAsync(int count)
        {
            return await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Images.OrderBy(i => i.SortOrder))
                .Where(p => p.IsFeatured)
                .OrderByDescending(p => p.AverageRating)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Product>> ListDeletedAsync()
        {
            return await _db.Products.IgnoreQueryFilters()
                .Where(p => p.IsDeleted)
                .Include(p => p.Category)
                .Include(p => p.Images.OrderBy(i => i.SortOrder))
                .Include(p => p.Specifications)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddImagesAsync(Guid productId, IReadOnlyList<ProductImage> images)
        {
            var existingCount = await _db.ProductImages.CountAsync(i => i.ProductId == productId);
            var sortOrder = existingCount;
            foreach (var image in images)
            {
                image.ProductId = productId;
                if (image.SortOrder < 0)
                {
                    image.SortOrder = sortOrder++;
                }
                _db.ProductImages.Add(image);
            }

            await _db.SaveChangesAsync();
        }

        public async Task AddSpecificationAsync(Guid productId, ProductSpecification specification)
        {
            specification.ProductId = productId;
            _db.ProductSpecifications.Add(specification);
            await _db.SaveChangesAsync();
        }

        public async Task AddReviewAsync(Guid productId, ProductReview review)
        {
            review.ProductId = productId;
            review.CreatedAt = DateTime.UtcNow;
            _db.ProductReviews.Add(review);
            
            var product = await _db.Products.FindAsync(productId);
            if (product is not null)
            {
                var reviews = await _db.ProductReviews.Where(r => r.ProductId == productId).ToListAsync();
                product.ReviewCount = reviews.Count + 1;
                product.AverageRating = reviews.Any() ? (reviews.Sum(r => r.Rating) + review.Rating) / (decimal)product.ReviewCount : review.Rating;
            }
            
            await _db.SaveChangesAsync();
        }

        public async Task RemoveSpecificationAsync(Guid specificationId)
        {
            var spec = await _db.ProductSpecifications.FindAsync(specificationId);
            if (spec is null)
            {
                return;
            }

            _db.ProductSpecifications.Remove(spec);
            await _db.SaveChangesAsync();
        }

        public async Task<ProductImage?> GetImageByIdAsync(Guid imageId)
        {
            return await _db.ProductImages.IgnoreQueryFilters().FirstOrDefaultAsync(i => i.Id == imageId);
        }

        public async Task RemoveImageAsync(Guid imageId)
        {
            var image = await _db.ProductImages.FindAsync(imageId);
            if (image is null)
            {
                return;
            }

            _db.ProductImages.Remove(image);
            await _db.SaveChangesAsync();
        }

        public async Task MoveImageAsync(Guid imageId, int direction)
        {
            var image = await _db.ProductImages.FirstOrDefaultAsync(i => i.Id == imageId);
            if (image is null)
            {
                return;
            }

            var images = await _db.ProductImages
                .Where(i => i.ProductId == image.ProductId)
                .OrderBy(i => i.SortOrder)
                .ToListAsync();

            var index = images.FindIndex(i => i.Id == imageId);
            var swapIndex = index + (direction < 0 ? -1 : 1);
            if (index < 0 || swapIndex < 0 || swapIndex >= images.Count)
            {
                return;
            }

            var current = images[index];
            var other = images[swapIndex];
            (current.SortOrder, other.SortOrder) = (other.SortOrder, current.SortOrder);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateImageOrderAsync(Guid productId, IReadOnlyDictionary<Guid, int> orderMap)
        {
            var images = await _db.ProductImages
                .Where(i => i.ProductId == productId)
                .ToListAsync();

            var maxOrder = orderMap.Count;

            foreach (var image in images)
            {
                if (orderMap.TryGetValue(image.Id, out var index))
                {
                    image.SortOrder = index;
                }
                else
                {
                    image.SortOrder = maxOrder++;
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task AddAsync(Product product)
        {
            _db.Products.Add(product);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _db.Products.Update(product);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _db.Products.FindAsync(id);
            if (entity != null)
            {
                _db.Products.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }

        public async Task RestoreAsync(Guid id)
        {
            var entity = await _db.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id);
            if (entity != null)
            {
                entity.IsDeleted = false;
                entity.DeletedAt = null;
                await _db.SaveChangesAsync();
            }
        }
    }
}