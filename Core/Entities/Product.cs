using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public class Product : Interfaces.ISoftDelete
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }
        public List<ProductImage> Images { get; set; } = [];
        public List<ProductSpecification> Specifications { get; set; } = [];
        public List<ProductReview> Reviews { get; set; } = [];
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(2000)]
        public string? Description { get; set; }
        
        [Range(0.01, 999999.99)]
        public decimal Price { get; set; }
        
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
        
        [Range(0, int.MaxValue)]
        public int ReservedStock { get; set; }
        
        public int AvailableStock => Stock - ReservedStock;
        
        [Range(0, int.MaxValue)]
        public int ReorderLevel { get; set; }
        
        [Range(0, int.MaxValue)]
        public int ReorderQuantity { get; set; }
        
        public bool LowStockAlert => AvailableStock <= ReorderLevel;
        public bool IsFeatured { get; set; }
        
        [Range(0, 5)]
        public decimal AverageRating { get; set; }
        
        [Range(0, int.MaxValue)]
        public int ReviewCount { get; set; }
        
        public byte[] RowVersion { get; set; } = [];
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}