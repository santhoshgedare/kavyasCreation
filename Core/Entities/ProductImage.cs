namespace Core.Entities
{
    public class ProductImage : Interfaces.ISoftDelete
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string Url { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
