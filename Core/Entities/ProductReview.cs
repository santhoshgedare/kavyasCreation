namespace Core.Entities
{
    public class ProductReview : Interfaces.ISoftDelete
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
