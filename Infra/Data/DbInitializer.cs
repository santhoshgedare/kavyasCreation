using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infra.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            await db.Database.EnsureCreatedAsync();
            const string clonePrefix = "[SeedClone]";

            if (db.Products.Any())
            {
                if (await db.Products.AnyAsync(p => p.Name.StartsWith(clonePrefix)))
                {
                    return;
                }

                await AddClonedProductsAsync(db, clonePrefix, 100);
                return;
            }

            var electronics = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Electronics",
                Description = "Devices and accessories"
            };
            var accessories = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Accessories",
                Description = "Bags, cases, and more"
            };

            db.Categories.AddRange(electronics, accessories);

            var headphones = new Product
            {
                Id = Guid.NewGuid(),
                CategoryId = electronics.Id,
                Name = "Wireless Headphones",
                Description = "Noise-cancelling over-ear headphones.",
                Price = 129.99m,
                Stock = 25,
                ReorderLevel = 10,
                ReorderQuantity = 20
            };
            var watch = new Product
            {
                Id = Guid.NewGuid(),
                CategoryId = electronics.Id,
                Name = "Smart Watch",
                Description = "Fitness tracking and notifications.",
                Price = 89.00m,
                Stock = 40,
                ReorderLevel = 15,
                ReorderQuantity = 30
            };
            var backpack = new Product
            {
                Id = Guid.NewGuid(),
                CategoryId = accessories.Id,
                Name = "Laptop Backpack",
                Description = "Water-resistant, 15-inch laptop support.",
                Price = 39.50m,
                Stock = 60,
                ReorderLevel = 20,
                ReorderQuantity = 40
            };

            db.Products.AddRange(headphones, watch, backpack);

            db.ProductImages.AddRange(
                new ProductImage { Id = Guid.NewGuid(), ProductId = headphones.Id, Url = "/uploads/products/headphones.svg", SortOrder = 0 },
                new ProductImage { Id = Guid.NewGuid(), ProductId = watch.Id, Url = "/uploads/products/watch.svg", SortOrder = 0 },
                new ProductImage { Id = Guid.NewGuid(), ProductId = backpack.Id, Url = "/uploads/products/backpack.svg", SortOrder = 0 }
            );

            db.ProductSpecifications.AddRange(
                new ProductSpecification { Id = Guid.NewGuid(), ProductId = headphones.Id, Key = "Battery", Value = "30 hours" },
                new ProductSpecification { Id = Guid.NewGuid(), ProductId = headphones.Id, Key = "Connectivity", Value = "Bluetooth 5.3" },
                new ProductSpecification { Id = Guid.NewGuid(), ProductId = watch.Id, Key = "Water Resistance", Value = "5 ATM" },
                new ProductSpecification { Id = Guid.NewGuid(), ProductId = watch.Id, Key = "Display", Value = "AMOLED" },
                new ProductSpecification { Id = Guid.NewGuid(), ProductId = backpack.Id, Key = "Capacity", Value = "22L" }
            );

            await db.SaveChangesAsync();

            await AddClonedProductsAsync(db, clonePrefix, 100);
        }

        private static async Task AddClonedProductsAsync(AppDbContext db, string clonePrefix, int cloneCount)
        {
            var sourceProducts = await db.Products
                .Include(p => p.Images)
                .Include(p => p.Specifications)
                .AsNoTracking()
                .ToListAsync();

            if (sourceProducts.Count == 0)
            {
                return;
            }

            var newProducts = new List<Product>(cloneCount);
            var newImages = new List<ProductImage>();
            var newSpecifications = new List<ProductSpecification>();

            for (var i = 0; i < cloneCount; i++)
            {
                var source = sourceProducts[i % sourceProducts.Count];
                var newProductId = Guid.NewGuid();
                var clone = new Product
                {
                    Id = newProductId,
                    CategoryId = source.CategoryId,
                    VendorId = source.VendorId,
                    Name = $"{clonePrefix} {source.Name} {i + 1}",
                    Description = source.Description,
                    Price = source.Price,
                    Stock = source.Stock,
                    ReservedStock = 0,
                    ReorderLevel = source.ReorderLevel,
                    ReorderQuantity = source.ReorderQuantity,
                    IsFeatured = source.IsFeatured,
                    AverageRating = source.AverageRating,
                    ReviewCount = 0
                };

                newProducts.Add(clone);

                foreach (var image in source.Images.OrderBy(i => i.SortOrder))
                {
                    newImages.Add(new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ProductId = newProductId,
                        Url = image.Url,
                        SortOrder = image.SortOrder
                    });
                }

                foreach (var spec in source.Specifications)
                {
                    newSpecifications.Add(new ProductSpecification
                    {
                        Id = Guid.NewGuid(),
                        ProductId = newProductId,
                        Key = spec.Key,
                        Value = spec.Value
                    });
                }
            }

            db.Products.AddRange(newProducts);
            if (newImages.Count > 0)
            {
                db.ProductImages.AddRange(newImages);
            }

            if (newSpecifications.Count > 0)
            {
                db.ProductSpecifications.AddRange(newSpecifications);
            }

            await db.SaveChangesAsync();
        }
    }
}
