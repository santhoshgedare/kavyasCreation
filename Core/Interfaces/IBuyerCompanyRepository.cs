using Core.Entities;

namespace Core.Interfaces
{
    public interface IBuyerCompanyRepository
    {
        Task<BuyerCompany?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<BuyerCompany>> GetAllAsync();
        Task<IReadOnlyList<BuyerCompany>> GetActiveCompaniesAsync();
        Task<IReadOnlyList<BuyerCompany>> GetApprovedCompaniesAsync();
        Task<BuyerCompany?> GetByCompanyNameAsync(string companyName);
        Task AddAsync(BuyerCompany buyerCompany);
        void Update(BuyerCompany buyerCompany);
        void Delete(BuyerCompany buyerCompany);
        Task<bool> CompanyNameExistsAsync(string companyName, Guid? excludeId = null);
    }
}
