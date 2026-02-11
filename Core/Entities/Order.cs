namespace Core.Entities
{
    public class Order : Interfaces.ISoftDelete
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        
        public Guid? BuyerCompanyId { get; set; } // Nullable - individual buyers or company buyers
        public BuyerCompany? BuyerCompany { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public decimal Total { get; set; }
        public List<OrderItem> Items { get; set; } = [];
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
