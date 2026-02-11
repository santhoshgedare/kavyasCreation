using Core.Entities;

namespace Core.Interfaces
{
    public interface IUserAddressRepository
    {
        Task<UserAddress?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<UserAddress>> GetByUserProfileIdAsync(Guid userProfileId);
        Task<UserAddress?> GetPrimaryAddressAsync(Guid userProfileId);
        Task<UserAddress?> GetShippingDefaultAsync(Guid userProfileId);
        Task AddAsync(UserAddress address);
        Task UpdateAsync(UserAddress address);
        Task DeleteAsync(Guid id);
        Task<bool> SetPrimaryAsync(Guid addressId, Guid userProfileId);
        Task<bool> SetShippingDefaultAsync(Guid addressId, Guid userProfileId);
    }
}
