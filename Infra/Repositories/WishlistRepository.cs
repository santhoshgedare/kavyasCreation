using Core.Entities;
using Core.Interfaces;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly AppDbContext _db;

        public WishlistRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<Wishlist>> GetByUserIdAsync(string userId)
        {
            return await _db.Wishlists
                .Include(w => w.Product)
                    .ThenInclude(p => p!.Images.OrderBy(i => i.SortOrder))
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();
        }

        public async Task<Wishlist?> GetByUserAndProductAsync(string userId, Guid productId)
        {
            return await _db.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
        }

        public async Task AddAsync(Wishlist wishlist)
        {
            wishlist.Id = Guid.NewGuid();
            wishlist.AddedAt = DateTime.UtcNow;
            _db.Wishlists.Add(wishlist);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveAsync(Guid id)
        {
            var wishlist = await _db.Wishlists.FindAsync(id);
            if (wishlist is not null)
            {
                _db.Wishlists.Remove(wishlist);
                await _db.SaveChangesAsync();
            }
        }
    }
}
