using Core.Entities;
using Core.Interfaces;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class BuyerCompanyRepository : IBuyerCompanyRepository
    {
        private readonly AppDbContext _db;
        
        public BuyerCompanyRepository(AppDbContext db) => _db = db;
        
        public async Task<BuyerCompany?> GetByIdAsync(Guid id) => await _db.BuyerCompanies
            .Include(bc => bc.BuyerUsers)
            .Include(bc => bc.VendorRelationships)
            .Include(bc => bc.Orders)
            .FirstOrDefaultAsync(bc => bc.Id == id);
        
        public async Task<IReadOnlyList<BuyerCompany>> GetAllAsync() => await _db.BuyerCompanies
            .Where(bc => !bc.IsDeleted)
            .AsNoTracking()
            .ToListAsync();
        
        public async Task<IReadOnlyList<BuyerCompany>> GetActiveCompaniesAsync() => await _db.BuyerCompanies
            .Where(bc => bc.IsActive && !bc.IsDeleted)
            .AsNoTracking()
            .ToListAsync();
        
        public async Task<IReadOnlyList<BuyerCompany>> GetApprovedCompaniesAsync() => await _db.BuyerCompanies
            .Where(bc => bc.IsApproved && bc.IsActive && !bc.IsDeleted)
            .AsNoTracking()
            .ToListAsync();
        
        public async Task<BuyerCompany?> GetByCompanyNameAsync(string companyName) => await _db.BuyerCompanies
            .FirstOrDefaultAsync(bc => bc.CompanyName == companyName && !bc.IsDeleted);
        
        public async Task AddAsync(BuyerCompany buyerCompany) => await _db.BuyerCompanies.AddAsync(buyerCompany);
        
        public void Update(BuyerCompany buyerCompany) => _db.BuyerCompanies.Update(buyerCompany);
        
        public void Delete(BuyerCompany buyerCompany)
        {
            buyerCompany.IsDeleted = true;
            buyerCompany.DeletedAt = DateTime.UtcNow;
            Update(buyerCompany);
        }
        
        public async Task<bool> CompanyNameExistsAsync(string companyName, Guid? excludeId = null)
        {
            var query = _db.BuyerCompanies.Where(bc => bc.CompanyName == companyName && !bc.IsDeleted);
            if (excludeId.HasValue)
            {
                query = query.Where(bc => bc.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }
    }
}
