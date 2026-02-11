using System.ComponentModel.DataAnnotations;
using Core.Interfaces;

namespace Core.Entities
{
    /// <summary>
    /// Represents a vendor company that can sell products on the platform
    /// </summary>
    public class Vendor : ISoftDelete
    {
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public required string CompanyName { get; set; }
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        [MaxLength(256)]
        [EmailAddress]
        public required string ContactEmail { get; set; }
        
        [MaxLength(20)]
        [Phone]
        public string? ContactPhone { get; set; }
        
        [MaxLength(500)]
        public string? Address { get; set; }
        
        [MaxLength(100)]
        public string? City { get; set; }
        
        [MaxLength(100)]
        public string? State { get; set; }
        
        [MaxLength(20)]
        public string? PostalCode { get; set; }
        
        [MaxLength(100)]
        public string? Country { get; set; }
        
        [MaxLength(100)]
        public string? TaxId { get; set; }
        
        [MaxLength(100)]
        public string? RegistrationNumber { get; set; }
        
        public bool IsActive { get; set; } = true;
        public bool IsApproved { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; } // Admin user ID who approved
        
        public DateTime? LastUpdatedAt { get; set; }
        
        // ISoftDelete
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        // Navigation properties
        public List<VendorUser> VendorUsers { get; set; } = [];
        public List<Product> Products { get; set; } = [];
        public List<VendorBuyerRelationship> BuyerRelationships { get; set; } = [];
    }
}
