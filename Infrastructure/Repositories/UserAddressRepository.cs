using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserAddressRepository : IUserAddressRepository
    {
        private readonly AppDbContext _db;

        public UserAddressRepository(AppDbContext db) => _db = db;

        public async Task<UserAddress?> GetByIdAsync(Guid id)
        {
            return await _db.UserAddresses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        }

        public async Task<IReadOnlyList<UserAddress>> GetByUserProfileIdAsync(Guid userProfileId)
        {
            return await _db.UserAddresses
                .Where(a => a.UserProfileId == userProfileId && !a.IsDeleted)
                .AsNoTracking()
                .OrderByDescending(a => a.IsPrimary)
                .ThenByDescending(a => a.IsShippingDefault)
                .ThenBy(a => a.Label)
                .ToListAsync();
        }

        public async Task<UserAddress?> GetPrimaryAddressAsync(Guid userProfileId)
        {
            return await _db.UserAddresses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserProfileId == userProfileId && a.IsPrimary && !a.IsDeleted);
        }

        public async Task<UserAddress?> GetShippingDefaultAsync(Guid userProfileId)
        {
            return await _db.UserAddresses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserProfileId == userProfileId && a.IsShippingDefault && !a.IsDeleted);
        }

        public async Task AddAsync(UserAddress address)
        {
            address.CreatedAt = DateTime.UtcNow;
            _db.UserAddresses.Add(address);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserAddress address)
        {
            address.LastUpdatedAt = DateTime.UtcNow;
            _db.UserAddresses.Update(address);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var address = await _db.UserAddresses.FindAsync(id);
            if (address != null)
            {
                _db.UserAddresses.Remove(address);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> SetPrimaryAsync(Guid addressId, Guid userProfileId)
        {
            // Remove primary from all other addresses
            var addresses = await _db.UserAddresses
                .Where(a => a.UserProfileId == userProfileId && !a.IsDeleted)
                .ToListAsync();

            foreach (var addr in addresses)
            {
                addr.IsPrimary = addr.Id == addressId;
                addr.LastUpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetShippingDefaultAsync(Guid addressId, Guid userProfileId)
        {
            // Remove shipping default from all other addresses
            var addresses = await _db.UserAddresses
                .Where(a => a.UserProfileId == userProfileId && !a.IsDeleted)
                .ToListAsync();

            foreach (var addr in addresses)
            {
                addr.IsShippingDefault = addr.Id == addressId;
                addr.LastUpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            return true;
        }
    }
}
