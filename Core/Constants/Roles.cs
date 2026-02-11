namespace Core.Constants
{
    /// <summary>
    /// Defines all role names used in the application
    /// </summary>
    public static class Roles
    {
        // System Administrator
        public const string Admin = "Admin";
        
        // Vendor roles
        public const string VendorAdmin = "VendorAdmin";
        public const string VendorUser = "VendorUser";
        
        // Buyer roles
        public const string BuyerAdmin = "BuyerAdmin";
        public const string BuyerUser = "BuyerUser";
        
        // Individual customer (not part of any company)
        public const string Customer = "Customer";
        
        /// <summary>
        /// Gets all role names
        /// </summary>
        public static readonly string[] All = 
        {
            Admin,
            VendorAdmin,
            VendorUser,
            BuyerAdmin,
            BuyerUser,
            Customer
        };
        
        /// <summary>
        /// Gets all vendor-related roles
        /// </summary>
        public static readonly string[] VendorRoles = 
        {
            VendorAdmin,
            VendorUser
        };
        
        /// <summary>
        /// Gets all buyer-related roles
        /// </summary>
        public static readonly string[] BuyerRoles = 
        {
            BuyerAdmin,
            BuyerUser
        };
    }
}
