using Core.Entities;

namespace Core.Interfaces
{
    public interface IVendorRepository
    {
        Task<Vendor?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Vendor>> GetAllAsync();
        Task<IReadOnlyList<Vendor>> GetActiveVendorsAsync();
        Task<IReadOnlyList<Vendor>> GetApprovedVendorsAsync();
        Task<Vendor?> GetByCompanyNameAsync(string companyName);
        Task AddAsync(Vendor vendor);
        void Update(Vendor vendor);
        void Delete(Vendor vendor);
        Task<bool> CompanyNameExistsAsync(string companyName, Guid? excludeId = null);
    }
}
