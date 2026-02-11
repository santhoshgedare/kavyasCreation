namespace Core.Entities
{
    public class Order : Interfaces.ISoftDelete
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal Total { get; set; }
        public List<OrderItem> Items { get; set; } = [];
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
