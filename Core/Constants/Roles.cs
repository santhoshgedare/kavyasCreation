namespace Core.Constants
{
    /// <summary>
    /// Defines all role names used in the application
    /// </summary>
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Customer = "Customer";

        /// <summary>
        /// Gets all role names
        /// </summary>
        public static readonly string[] All = [Admin, Customer];
    }
}
