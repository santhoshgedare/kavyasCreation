using System.ComponentModel.DataAnnotations;
using Core.Interfaces;

namespace Core.Entities
{
    /// <summary>
    /// Represents a user who belongs to a buyer company
    /// </summary>
    public class BuyerUser : ISoftDelete
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid BuyerCompanyId { get; set; }
        public BuyerCompany? BuyerCompany { get; set; }
        
        [Required]
        [MaxLength(450)]
        public required string UserId { get; set; } // Foreign key to AspNetUsers
        
        [Required]
        [MaxLength(100)]
        public required string FirstName { get; set; }
        
        [Required]
        [MaxLength(100)]
        public required string LastName { get; set; }
        
        [Required]
        [MaxLength(256)]
        [EmailAddress]
        public required string Email { get; set; }
        
        [MaxLength(20)]
        [Phone]
        public string? PhoneNumber { get; set; }
        
        [MaxLength(100)]
        public string? JobTitle { get; set; }
        
        [MaxLength(100)]
        public string? Department { get; set; }
        
        public bool IsAdmin { get; set; } = false; // Buyer admin or regular buyer user
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdatedAt { get; set; }
        
        // ISoftDelete
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        // Computed property
        public string FullName => $"{FirstName} {LastName}";
    }
}
