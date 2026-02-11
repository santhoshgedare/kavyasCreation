using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public class Category : Interfaces.ISoftDelete
    {
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
