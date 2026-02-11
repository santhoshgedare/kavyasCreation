namespace Core.Interfaces
{
    public interface IUnitOfWork
    {
        IProductRepository Products { get; }
        ICategoryRepository Categories { get; }
        IOrderRepository Orders { get; }
        IWishlistRepository Wishlists { get; }
        IUserProfileRepository UserProfiles { get; }
        IUserAddressRepository UserAddresses { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
