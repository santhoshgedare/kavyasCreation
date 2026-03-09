using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    /// <summary>
    /// Defines a variant dimension expected for products in a category (e.g., Size, Color).
    /// </summary>
    public class CategoryVariantType
    {
        public Guid Id { get; set; }

        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int SortOrder { get; set; }
    }
}
