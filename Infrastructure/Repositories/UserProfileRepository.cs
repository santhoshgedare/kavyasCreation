using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly AppDbContext _db;

        public UserProfileRepository(AppDbContext db) => _db = db;

        public async Task<UserProfile?> GetByIdAsync(Guid id)
        {
            return await _db.UserProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        }

        public async Task<UserProfile?> GetByUserIdAsync(string userId)
        {
            return await _db.UserProfiles
                .Include(u => u.Addresses)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
        }

        public async Task<IReadOnlyList<UserProfile>> ListAllAsync()
        {
            return await _db.UserProfiles
                .Include(u => u.Addresses)
                .Where(u => !u.IsDeleted)
                .AsNoTracking()
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }

        public async Task<UserProfile?> GetByEmailAsync(string email)
        {
            return await _db.UserProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        }

        public async Task<bool> ExistsAsync(string userId)
        {
            return await _db.UserProfiles
                .AnyAsync(u => u.UserId == userId && !u.IsDeleted);
        }

        public async Task AddAsync(UserProfile userProfile)
        {
            userProfile.CreatedAt = DateTime.UtcNow;
            _db.UserProfiles.Add(userProfile);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserProfile userProfile)
        {
            userProfile.LastUpdatedAt = DateTime.UtcNow;
            _db.UserProfiles.Update(userProfile);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _db.UserProfiles.FindAsync(id);
            if (entity != null)
            {
                _db.UserProfiles.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }
    }
}
