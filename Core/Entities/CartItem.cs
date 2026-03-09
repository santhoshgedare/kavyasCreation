namespace Core.Entities
{
    public class CartItem
    {
        public Guid ProductId { get; set; }
        public Guid? ProductVariantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? VariantDescription { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
    }
}
