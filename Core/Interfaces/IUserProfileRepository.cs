using Core.Entities;

namespace Core.Interfaces
{
    public interface IUserProfileRepository
    {
        Task<UserProfile?> GetByIdAsync(Guid id);
        Task<UserProfile?> GetByUserIdAsync(string userId);
        Task<IReadOnlyList<UserProfile>> ListAllAsync();
        Task<UserProfile?> GetByEmailAsync(string email);
        Task<bool> ExistsAsync(string userId);
        Task AddAsync(UserProfile userProfile);
        Task UpdateAsync(UserProfile userProfile);
        Task DeleteAsync(Guid id);
    }
}
