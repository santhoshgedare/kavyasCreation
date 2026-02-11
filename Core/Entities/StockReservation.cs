namespace Core.Entities
{
    public class StockReservation
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime ReservedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public Guid? OrderId { get; set; }
        public bool IsCommitted { get; set; }
        public bool IsCancelled { get; set; }
    }
}
