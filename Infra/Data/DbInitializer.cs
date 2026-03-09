using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infra.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            await db.Database.EnsureCreatedAsync();

            // Only seed categories if they don't exist
            if (await db.Categories.AnyAsync())
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
            await db.SaveChangesAsync();
        }
    }
}
