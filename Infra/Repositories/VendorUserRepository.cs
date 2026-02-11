using Core.Entities;
using Core.Interfaces;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class VendorUserRepository : IVendorUserRepository
    {
        private readonly AppDbContext _db;
        
        public VendorUserRepository(AppDbContext db) => _db = db;
        
        public async Task<VendorUser?> GetByIdAsync(Guid id) => await _db.VendorUsers
            .Include(vu => vu.Vendor)
            .FirstOrDefaultAsync(vu => vu.Id == id);
        
        public async Task<VendorUser?> GetByUserIdAsync(string userId) => await _db.VendorUsers
            .Include(vu => vu.Vendor)
            .FirstOrDefaultAsync(vu => vu.UserId == userId && !vu.IsDeleted);
        
        public async Task<IReadOnlyList<VendorUser>> GetByVendorIdAsync(Guid vendorId) => await _db.VendorUsers
            .Where(vu => vu.VendorId == vendorId && !vu.IsDeleted)
            .AsNoTracking()
            .ToListAsync();
        
        public async Task<IReadOnlyList<VendorUser>> GetActiveByVendorIdAsync(Guid vendorId) => await _db.VendorUsers
            .Where(vu => vu.VendorId == vendorId && vu.IsActive && !vu.IsDeleted)
            .AsNoTracking()
            .ToListAsync();
        
        public async Task AddAsync(VendorUser vendorUser) => await _db.VendorUsers.AddAsync(vendorUser);
        
        public void Update(VendorUser vendorUser) => _db.VendorUsers.Update(vendorUser);
        
        public void Delete(VendorUser vendorUser)
        {
            vendorUser.IsDeleted = true;
            vendorUser.DeletedAt = DateTime.UtcNow;
            Update(vendorUser);
        }
        
        public async Task<bool> UserExistsInVendorAsync(string userId, Guid vendorId) =>
            await _db.VendorUsers.AnyAsync(vu => vu.UserId == userId && vu.VendorId == vendorId && !vu.IsDeleted);
    }
}
