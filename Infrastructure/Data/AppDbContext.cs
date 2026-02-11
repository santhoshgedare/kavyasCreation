using System.Linq.Expressions;
using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<ProductSpecification> ProductSpecifications => Set<ProductSpecification>();
        public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
        public DbSet<Wishlist> Wishlists => Set<Wishlist>();
        public DbSet<StockReservation> StockReservations => Set<StockReservation>();
        public DbSet<StockMovement> StockMovements => Set<StockMovement>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<UserAddress> UserAddresses => Set<UserAddress>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Order>(entity =>
            {
                entity.HasMany(o => o.Items)
                    .WithOne()
                    .HasForeignKey(i => i.OrderId);
                entity.Property(o => o.Total).HasPrecision(18, 2);
            });

            builder.Entity<Product>(entity =>
            {
                entity.HasOne(p => p.Category)
                    .WithMany()
                    .HasForeignKey(p => p.CategoryId);
                entity.HasMany(p => p.Images)
                    .WithOne()
                    .HasForeignKey(i => i.ProductId);
                entity.HasMany(p => p.Specifications)
                    .WithOne()
                    .HasForeignKey(s => s.ProductId);
                entity.HasMany(p => p.Reviews)
                    .WithOne()
                    .HasForeignKey(r => r.ProductId);
                entity.Property(p => p.Price).HasPrecision(18, 2);
                entity.Property(p => p.AverageRating).HasPrecision(3, 2);
                entity.Property(p => p.RowVersion).IsRowVersion();
                entity.Ignore(p => p.AvailableStock);
                entity.Ignore(p => p.LowStockAlert);
            });

            builder.Entity<ProductImage>(entity =>
            {
                entity.Property(i => i.Url).HasMaxLength(500);
                entity.Property(i => i.SortOrder).HasDefaultValue(0);
            });

            builder.Entity<ProductSpecification>(entity =>
            {
                entity.Property(s => s.Key).HasMaxLength(200);
                entity.Property(s => s.Value).HasMaxLength(500);
            });

            builder.Entity<Category>(entity =>
            {
                entity.Property(c => c.Name).HasMaxLength(200);
            });

            builder.Entity<OrderItem>(entity =>
            {
                entity.Property(i => i.Price).HasPrecision(18, 2);
            });

            builder.Entity<ProductReview>(entity =>
            {
                entity.Property(r => r.UserName).HasMaxLength(256);
                entity.Property(r => r.Comment).HasMaxLength(2000);
            });

            builder.Entity<Wishlist>(entity =>
            {
                entity.HasOne(w => w.Product)
                    .WithMany()
                    .HasForeignKey(w => w.ProductId);
                entity.HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();
            });

            builder.Entity<StockReservation>(entity =>
            {
                entity.HasOne(r => r.Product)
                    .WithMany()
                    .HasForeignKey(r => r.ProductId);
                entity.HasIndex(r => new { r.ProductId, r.IsCommitted, r.IsCancelled });
                entity.HasIndex(r => r.ExpiresAt);
            });

            builder.Entity<StockMovement>(entity =>
            {
                entity.HasOne(m => m.Product)
                    .WithMany()
                    .HasForeignKey(m => m.ProductId);
                entity.HasIndex(m => new { m.ProductId, m.CreatedAt });
                entity.Property(m => m.Notes).HasMaxLength(500);
            });

            builder.Entity<UserProfile>(entity =>
            {
                // Unique index on UserId to ensure one profile per user
                entity.HasIndex(u => u.UserId).IsUnique();
                entity.HasIndex(u => u.Email);
                
                // Property configurations
                entity.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
                entity.Property(u => u.LastName).HasMaxLength(100).IsRequired();
                entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
                entity.Property(u => u.UserId).HasMaxLength(450).IsRequired();
                
                // Note: FK to AspNetUsers is handled at application level
                // because AspNetUsers is in ApplicationDbContext (different context)
                
                // Navigation to addresses
                entity.HasMany(u => u.Addresses)
                    .WithOne(a => a.UserProfile)
                    .HasForeignKey(a => a.UserProfileId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<UserAddress>(entity =>
            {
                entity.HasIndex(a => new { a.UserProfileId, a.IsPrimary });
                entity.HasIndex(a => new { a.UserProfileId, a.IsShippingDefault });
                entity.Property(a => a.Label).HasMaxLength(200).IsRequired();
                entity.Property(a => a.AddressLine1).HasMaxLength(500).IsRequired();
                entity.Property(a => a.City).HasMaxLength(100).IsRequired();
                entity.Property(a => a.State).HasMaxLength(100).IsRequired();
                entity.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
                entity.Property(a => a.Country).HasMaxLength(100).IsRequired();
            });

            ApplySoftDeleteFilters(builder);
        }

        public override int SaveChanges()
        {
            ApplySoftDeleteBehavior();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplySoftDeleteBehavior();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplySoftDeleteBehavior()
        {
            foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                }
            }
        }

        private static void ApplySoftDeleteFilters(ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (!typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    continue;
                }

                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                var condition = Expression.Equal(property, Expression.Constant(false));
                var lambda = Expression.Lambda(condition, parameter);
                builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}
