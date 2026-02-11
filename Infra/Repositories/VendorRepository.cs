using Core.Entities;
using Core.Interfaces;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class VendorRepository : IVendorRepository
    {
        private readonly AppDbContext _db;
        
        public VendorRepository(AppDbContext db) => _db = db;
        
        public async Task<Vendor?> GetByIdAsync(Guid id) => await _db.Vendors
            .Include(v => v.VendorUsers)
            .Include(v => v.Products)
            .Include(v => v.BuyerRelationships)
            .FirstOrDefaultAsync(v => v.Id == id);
        
        public async Task<IReadOnlyList<Vendor>> GetAllAsync() => await _db.Vendors
            .Where(v => !v.IsDeleted)
            .AsNoTracking()
            .ToListAsync();
        
        public async Task<IReadOnlyList<Vendor>> GetActiveVendorsAsync() => await _db.Vendors
            .Where(v => v.IsActive && !v.IsDeleted)
            .AsNoTracking()
            .ToListAsync();
        
        public async Task<IReadOnlyList<Vendor>> GetApprovedVendorsAsync() => await _db.Vendors
            .Where(v => v.IsApproved && v.IsActive && !v.IsDeleted)
            .AsNoTracking()
            .ToListAsync();
        
        public async Task<Vendor?> GetByCompanyNameAsync(string companyName) => await _db.Vendors
            .FirstOrDefaultAsync(v => v.CompanyName == companyName && !v.IsDeleted);
        
        public async Task AddAsync(Vendor vendor) => await _db.Vendors.AddAsync(vendor);
        
        public void Update(Vendor vendor) => _db.Vendors.Update(vendor);
        
        public void Delete(Vendor vendor)
        {
            vendor.IsDeleted = true;
            vendor.DeletedAt = DateTime.UtcNow;
            Update(vendor);
        }
        
        public async Task<bool> CompanyNameExistsAsync(string companyName, Guid? excludeId = null)
        {
            var query = _db.Vendors.Where(v => v.CompanyName == companyName && !v.IsDeleted);
            if (excludeId.HasValue)
            {
                query = query.Where(v => v.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }
    }
}
