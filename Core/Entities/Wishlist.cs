namespace Core.Entities
{
    public class Wishlist : Interfaces.ISoftDelete
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        public DateTime AddedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
