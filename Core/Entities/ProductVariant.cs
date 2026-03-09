using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    /// <summary>
    /// A purchasable variant of a product with its own price and stock.
    /// Example: "Small / Gold" variant of a bangle.
    /// </summary>
    public class ProductVariant
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        [MaxLength(100)]
        public string? SKU { get; set; }

        [Range(0.01, 999999.99)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public bool IsActive { get; set; } = true;

        public List<ProductVariantOption> Options { get; set; } = [];
        public List<ProductImage> Images { get; set; } = [];
    }
}
