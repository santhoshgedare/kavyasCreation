using Core.Interfaces;
using Infra.Data;

namespace Infra.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _db;

        public UnitOfWork(AppDbContext db, IProductRepository productRepository, IOrderRepository orderRepository, ICategoryRepository categoryRepository, IWishlistRepository wishlistRepository, IUserProfileRepository userProfileRepository, IUserAddressRepository userAddressRepository)
        {
            _db = db;
            Products = productRepository;
            Orders = orderRepository;
            Categories = categoryRepository;
            Wishlists = wishlistRepository;
            UserProfiles = userProfileRepository;
            UserAddresses = userAddressRepository;
        }

        public IProductRepository Products { get; }
        public ICategoryRepository Categories { get; }
        public IOrderRepository Orders { get; }
        public IWishlistRepository Wishlists { get; }
        public IUserProfileRepository UserProfiles { get; }
        public IUserAddressRepository UserAddresses { get; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _db.SaveChangesAsync(cancellationToken);
        }
    }
}
