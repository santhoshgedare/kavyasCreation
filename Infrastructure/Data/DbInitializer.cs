using Core.Entities;

namespace Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            await db.Database.EnsureCreatedAsync();

            if (db.Products.Any())
            {
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
        }
    }
}
