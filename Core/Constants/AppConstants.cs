namespace Core.Constants
{
    /// <summary>
    /// Constants for stock movement types used throughout the inventory system.
    /// </summary>
    public static class StockMovementTypes
    {
        /// <summary>
        /// Stock reserved for a pending order (temporary hold).
        /// </summary>
        public const string Reservation = "Reservation";
        
        /// <summary>
        /// Stock sold and removed from inventory (permanent deduction).
        /// </summary>
        public const string Sale = "Sale";
        
        /// <summary>
        /// Reserved stock released back to available inventory.
        /// </summary>
        public const string Release = "Release";
        
        /// <summary>
        /// Manual stock adjustment (correction, damage, etc.).
        /// </summary>
        public const string Adjustment = "Adjustment";
        
        /// <summary>
        /// Stock added through purchase or restocking.
        /// </summary>
        public const string Restock = "Restock";
        
        /// <summary>
        /// Stock removed due to damage or loss.
        /// </summary>
        public const string Damage = "Damage";
        
        /// <summary>
        /// Stock returned by customer.
        /// </summary>
        public const string Return = "Return";
    }

    /// <summary>
    /// System user identifiers for automated operations.
    /// </summary>
    public static class SystemUsers
    {
        /// <summary>
        /// System user for automated tasks (cleanup, expiration, etc.).
        /// </summary>
        public const string System = "System";
        
        /// <summary>
        /// Administrator role identifier.
        /// </summary>
        public const string Administrator = "Administrator";
    }

    /// <summary>
    /// Application-wide configuration constants.
    /// </summary>
    public static class AppConstants
    {
        /// <summary>
        /// Default stock reservation expiration time in minutes.
        /// </summary>
        public const int DefaultReservationExpirationMinutes = 15;
        
        /// <summary>
        /// Maximum items allowed in shopping cart.
        /// </summary>
        public const int MaxCartItems = 50;
        
        /// <summary>
        /// Default page size for paginated results.
        /// </summary>
        public const int DefaultPageSize = 20;
        
        /// <summary>
        /// Maximum page size for paginated results.
        /// </summary>
        public const int MaxPageSize = 100;
        
        /// <summary>
        /// Session timeout in minutes.
        /// </summary>
        public const int SessionTimeoutMinutes = 30;
    }

    /// <summary>
    /// Cache key prefixes for distributed caching.
    /// </summary>
    public static class CacheKeys
    {
        public const string Products = "products:";
        public const string Categories = "categories:";
        public const string Cart = "cart:";
        public const string UserProfile = "user:";
        public const string ProductList = "productlist:";
        
        /// <summary>
        /// Builds a cache key for a specific product.
        /// </summary>
        public static string Product(Guid id) => $"{Products}{id}";
        
        /// <summary>
        /// Builds a cache key for a specific category.
        /// </summary>
        public static string Category(Guid id) => $"{Categories}{id}";
        
        /// <summary>
        /// Builds a cache key for a user's cart.
        /// </summary>
        public static string UserCart(string userId) => $"{Cart}{userId}";
    }
}
