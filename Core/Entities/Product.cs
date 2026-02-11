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
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int ReservedStock { get; set; }
        public int AvailableStock => Stock - ReservedStock;
        public int ReorderLevel { get; set; }
        public int ReorderQuantity { get; set; }
        public bool LowStockAlert => AvailableStock <= ReorderLevel;
        public bool IsFeatured { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public byte[] RowVersion { get; set; } = [];
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}