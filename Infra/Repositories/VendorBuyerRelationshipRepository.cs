using Core.Entities;
using Core.Interfaces;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class VendorBuyerRelationshipRepository : IVendorBuyerRelationshipRepository
    {
        private readonly AppDbContext _db;
        
        public VendorBuyerRelationshipRepository(AppDbContext db) => _db = db;
        
        public async Task<VendorBuyerRelationship?> GetByIdAsync(Guid id) => await _db.VendorBuyerRelationships
            .Include(vbr => vbr.Vendor)
            .Include(vbr => vbr.BuyerCompany)
            .FirstOrDefaultAsync(vbr => vbr.Id == id);
        
        public async Task<VendorBuyerRelationship?> GetRelationshipAsync(Guid vendorId, Guid buyerCompanyId) => 
            await _db.VendorBuyerRelationships
                .Include(vbr => vbr.Vendor)
                .Include(vbr => vbr.BuyerCompany)
                .FirstOrDefaultAsync(vbr => vbr.VendorId == vendorId && vbr.BuyerCompanyId == buyerCompanyId);
        
        public async Task<IReadOnlyList<VendorBuyerRelationship>> GetByVendorIdAsync(Guid vendorId) => 
            await _db.VendorBuyerRelationships
                .Include(vbr => vbr.BuyerCompany)
                .Where(vbr => vbr.VendorId == vendorId)
                .AsNoTracking()
                .ToListAsync();
        
        public async Task<IReadOnlyList<VendorBuyerRelationship>> GetByBuyerCompanyIdAsync(Guid buyerCompanyId) => 
            await _db.VendorBuyerRelationships
                .Include(vbr => vbr.Vendor)
                .Where(vbr => vbr.BuyerCompanyId == buyerCompanyId)
                .AsNoTracking()
                .ToListAsync();
        
        public async Task<IReadOnlyList<VendorBuyerRelationship>> GetActiveRelationshipsByVendorIdAsync(Guid vendorId) => 
            await _db.VendorBuyerRelationships
                .Include(vbr => vbr.BuyerCompany)
                .Where(vbr => vbr.VendorId == vendorId && vbr.IsActive && vbr.IsApproved)
                .AsNoTracking()
                .ToListAsync();
        
        public async Task<IReadOnlyList<VendorBuyerRelationship>> GetActiveRelationshipsByBuyerCompanyIdAsync(Guid buyerCompanyId) => 
            await _db.VendorBuyerRelationships
                .Include(vbr => vbr.Vendor)
                .Where(vbr => vbr.BuyerCompanyId == buyerCompanyId && vbr.IsActive && vbr.IsApproved)
                .AsNoTracking()
                .ToListAsync();
        
        public async Task AddAsync(VendorBuyerRelationship relationship) => 
            await _db.VendorBuyerRelationships.AddAsync(relationship);
        
        public void Update(VendorBuyerRelationship relationship) => 
            _db.VendorBuyerRelationships.Update(relationship);
        
        public void Delete(VendorBuyerRelationship relationship) => 
            _db.VendorBuyerRelationships.Remove(relationship);
        
        public async Task<bool> RelationshipExistsAsync(Guid vendorId, Guid buyerCompanyId) =>
            await _db.VendorBuyerRelationships.AnyAsync(vbr => vbr.VendorId == vendorId && vbr.BuyerCompanyId == buyerCompanyId);
    }
}
