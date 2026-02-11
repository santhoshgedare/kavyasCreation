using Core.Entities;

namespace Core.Interfaces
{
    public interface IWishlistRepository
    {
        Task<IReadOnlyList<Wishlist>> GetByUserIdAsync(string userId);
        Task<Wishlist?> GetByUserAndProductAsync(string userId, Guid productId);
        Task AddAsync(Wishlist wishlist);
        Task RemoveAsync(Guid id);
    }
}
