using Core.Entities;

namespace kavyasCreation.Services
{
    public class CartService
    {
        private const string CartKey = "cart";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public List<CartItem> GetItems()
        {
            return Session.GetObject<List<CartItem>>(CartKey) ?? [];
        }

        public void AddItem(Product product, int quantity = 1)
        {
            var items = GetItems();
            var existing = items.FirstOrDefault(i => i.ProductId == product.Id);
            if (existing is null)
            {
                var imageUrl = product.Images.FirstOrDefault()?.Url ?? "/uploads/products/placeholder.svg";
                items.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = imageUrl
                });
            }
            else
            {
                existing.Quantity += quantity;
            }

            Session.SetObject(CartKey, items);
        }

        public void UpdateQuantity(Guid productId, int quantity)
        {
            var items = GetItems();
            var existing = items.FirstOrDefault(i => i.ProductId == productId);
            if (existing is not null)
            {
                existing.Quantity = Math.Max(1, quantity);
                Session.SetObject(CartKey, items);
            }
        }

        public void RemoveItem(Guid productId)
        {
            var items = GetItems();
            items.RemoveAll(i => i.ProductId == productId);
            Session.SetObject(CartKey, items);
        }

        public void Clear()
        {
            Session.Remove(CartKey);
        }

        public decimal GetTotal() => GetItems().Sum(i => i.Price * i.Quantity);
    }
}
