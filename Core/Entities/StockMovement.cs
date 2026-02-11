namespace Core.Entities
{
    public class StockMovement
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        public int StockBefore { get; set; }
        public int StockAfter { get; set; }
        public string MovementType { get; set; } = string.Empty; // Purchase, Sale, Adjustment, Return, Reservation, Release
        public string? ReferenceId { get; set; }
        public string? Notes { get; set; }
        public string PerformedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
