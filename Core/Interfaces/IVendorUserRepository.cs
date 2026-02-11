using Core.Entities;

namespace Core.Interfaces
{
    public interface IVendorUserRepository
    {
        Task<VendorUser?> GetByIdAsync(Guid id);
        Task<VendorUser?> GetByUserIdAsync(string userId);
        Task<IReadOnlyList<VendorUser>> GetByVendorIdAsync(Guid vendorId);
        Task<IReadOnlyList<VendorUser>> GetActiveByVendorIdAsync(Guid vendorId);
        Task AddAsync(VendorUser vendorUser);
        void Update(VendorUser vendorUser);
        void Delete(VendorUser vendorUser);
        Task<bool> UserExistsInVendorAsync(string userId, Guid vendorId);
    }
}
