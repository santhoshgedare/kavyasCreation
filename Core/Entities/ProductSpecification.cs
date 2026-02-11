namespace Core.Entities
{
    public class ProductSpecification : Interfaces.ISoftDelete
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
