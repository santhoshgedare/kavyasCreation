using System.ComponentModel.DataAnnotations;
using Core.Interfaces;

namespace Core.Entities
{
    public class UserProfile : ISoftDelete
    {
        public Guid Id { get; set; }

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
        public required string Email { get; set; }

        [MaxLength(256)]
        public string? AlternateEmail { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(20)]
        public string? AlternatePhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; } // Male, Female, Other

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdatedAt { get; set; }

        // ISoftDelete
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Navigation
        public List<UserAddress> Addresses { get; set; } = [];

        // Computed property
        public string FullName => $"{FirstName} {LastName}";
    }
}


