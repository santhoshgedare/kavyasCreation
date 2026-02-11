using System.ComponentModel.DataAnnotations;
using Core.Interfaces;

namespace Core.Entities
{
    public class UserAddress : ISoftDelete
    {
        public Guid Id { get; set; }

        [Required]
        public Guid UserProfileId { get; set; }
        public UserProfile? UserProfile { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Label { get; set; } // e.g., "Home", "Work", "Shipping"

        [Required]
        [MaxLength(500)]
        public required string AddressLine1 { get; set; }

        [MaxLength(500)]
        public string? AddressLine2 { get; set; }

        [Required]
        [MaxLength(100)]
        public required string City { get; set; }

        [Required]
        [MaxLength(100)]
        public required string State { get; set; }

        [Required]
        [MaxLength(20)]
        public required string PostalCode { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Country { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public bool IsPrimary { get; set; }
        public bool IsShippingDefault { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdatedAt { get; set; }

        // ISoftDelete
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Computed
        public string FullAddress => $"{AddressLine1}, {(string.IsNullOrWhiteSpace(AddressLine2) ? "" : AddressLine2 + ", ")}{City}, {State} {PostalCode}, {Country}";
    }
}
