using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    /// <summary>
    /// Represents the relationship between a buyer company and a vendor.
    /// A buyer company can have multiple vendors, and a vendor can have multiple buyer companies.
    /// </summary>
    public class VendorBuyerRelationship
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid VendorId { get; set; }
        public Vendor? Vendor { get; set; }
        
        [Required]
        public Guid BuyerCompanyId { get; set; }
        public BuyerCompany? BuyerCompany { get; set; }
        
        public bool IsActive { get; set; } = true;
        public bool IsApproved { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
        
        public string? ApprovedBy { get; set; } // User ID who approved the relationship
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        public DateTime? LastUpdatedAt { get; set; }
    }
}
