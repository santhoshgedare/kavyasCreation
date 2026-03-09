using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    /// <summary>
    /// A specific option value for a product variant tied to a category variant type.
    /// Example: CategoryVariantType = "Size", Value = "Small".
    /// </summary>
    public class ProductVariantOption
    {
        public Guid Id { get; set; }

        public Guid ProductVariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }

        public Guid CategoryVariantTypeId { get; set; }
        public CategoryVariantType? CategoryVariantType { get; set; }

        [Required]
        [MaxLength(200)]
        public string Value { get; set; } = string.Empty;
    }
}
