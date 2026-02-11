namespace Core.Interfaces
{
    public interface IUnitOfWork
    {
        // Existing repositories
        IProductRepository Products { get; }
        ICategoryRepository Categories { get; }
        IOrderRepository Orders { get; }
        IWishlistRepository Wishlists { get; }
        IUserProfileRepository UserProfiles { get; }
        IUserAddressRepository UserAddresses { get; }
        
        // New marketplace repositories
        IVendorRepository Vendors { get; }
        IVendorUserRepository VendorUsers { get; }
        IBuyerCompanyRepository BuyerCompanies { get; }
        IBuyerUserRepository BuyerUsers { get; }
        IVendorBuyerRelationshipRepository VendorBuyerRelationships { get; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
