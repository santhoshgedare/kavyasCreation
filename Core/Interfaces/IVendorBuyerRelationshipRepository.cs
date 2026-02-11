using Core.Entities;

namespace Core.Interfaces
{
    public interface IVendorBuyerRelationshipRepository
    {
        Task<VendorBuyerRelationship?> GetByIdAsync(Guid id);
        Task<VendorBuyerRelationship?> GetRelationshipAsync(Guid vendorId, Guid buyerCompanyId);
        Task<IReadOnlyList<VendorBuyerRelationship>> GetByVendorIdAsync(Guid vendorId);
        Task<IReadOnlyList<VendorBuyerRelationship>> GetByBuyerCompanyIdAsync(Guid buyerCompanyId);
        Task<IReadOnlyList<VendorBuyerRelationship>> GetActiveRelationshipsByVendorIdAsync(Guid vendorId);
        Task<IReadOnlyList<VendorBuyerRelationship>> GetActiveRelationshipsByBuyerCompanyIdAsync(Guid buyerCompanyId);
        Task AddAsync(VendorBuyerRelationship relationship);
        void Update(VendorBuyerRelationship relationship);
        void Delete(VendorBuyerRelationship relationship);
        Task<bool> RelationshipExistsAsync(Guid vendorId, Guid buyerCompanyId);
    }
}
