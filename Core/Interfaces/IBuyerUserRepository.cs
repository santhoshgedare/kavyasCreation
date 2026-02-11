using Core.Entities;

namespace Core.Interfaces
{
    public interface IBuyerUserRepository
    {
        Task<BuyerUser?> GetByIdAsync(Guid id);
        Task<BuyerUser?> GetByUserIdAsync(string userId);
        Task<IReadOnlyList<BuyerUser>> GetByBuyerCompanyIdAsync(Guid buyerCompanyId);
        Task<IReadOnlyList<BuyerUser>> GetActiveByBuyerCompanyIdAsync(Guid buyerCompanyId);
        Task AddAsync(BuyerUser buyerUser);
        void Update(BuyerUser buyerUser);
        void Delete(BuyerUser buyerUser);
        Task<bool> UserExistsInCompanyAsync(string userId, Guid buyerCompanyId);
    }
}
