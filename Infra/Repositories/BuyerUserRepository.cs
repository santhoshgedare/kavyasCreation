using Core.Entities;
using Core.Interfaces;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class BuyerUserRepository : IBuyerUserRepository
    {
        private readonly AppDbContext _db;
        
        public BuyerUserRepository(AppDbContext db) => _db = db;
        
        public async Task<BuyerUser?> GetByIdAsync(Guid id) => await _db.BuyerUsers
            .Include(bu => bu.BuyerCompany)
            .FirstOrDefaultAsync(bu => bu.Id == id);
        
        public async Task<BuyerUser?> GetByUserIdAsync(string userId) => await _db.BuyerUsers
            .Include(bu => bu.BuyerCompany)
            .FirstOrDefaultAsync(bu => bu.UserId == userId && !bu.IsDeleted);
        
        public async Task<IReadOnlyList<BuyerUser>> GetByBuyerCompanyIdAsync(Guid buyerCompanyId) => await _db.BuyerUsers
            .Where(bu => bu.BuyerCompanyId == buyerCompanyId && !bu.IsDeleted)
            .AsNoTracking()
            .ToListAsync();
        
        public async Task<IReadOnlyList<BuyerUser>> GetActiveByBuyerCompanyIdAsync(Guid buyerCompanyId) => await _db.BuyerUsers
            .Where(bu => bu.BuyerCompanyId == buyerCompanyId && bu.IsActive && !bu.IsDeleted)
            .AsNoTracking()
            .ToListAsync();
        
        public async Task AddAsync(BuyerUser buyerUser) => await _db.BuyerUsers.AddAsync(buyerUser);
        
        public void Update(BuyerUser buyerUser) => _db.BuyerUsers.Update(buyerUser);
        
        public void Delete(BuyerUser buyerUser)
        {
            buyerUser.IsDeleted = true;
            buyerUser.DeletedAt = DateTime.UtcNow;
            Update(buyerUser);
        }
        
        public async Task<bool> UserExistsInCompanyAsync(string userId, Guid buyerCompanyId) =>
            await _db.BuyerUsers.AnyAsync(bu => bu.UserId == userId && bu.BuyerCompanyId == buyerCompanyId && !bu.IsDeleted);
    }
}
