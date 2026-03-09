using Core.Entities;

namespace Web.Services
{
    /// <summary>
    /// Service for managing shopping cart operations stored in session.
    /// Cart data is persisted in the HTTP session and is not database-backed.
    /// </summary>
    public class CartService
    {
        private const string CartKey = "cart";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        /// <summary>
        /// Gets all items currently in the shopping cart.
        /// </summary>
        /// <returns>A list of cart items, or an empty list if cart is empty.</returns>
        public List<CartItem> GetItems()
        {
            return Session.GetObject<List<CartItem>>(CartKey) ?? [];
        }

        /// <summary>
        /// Adds a product to the cart or updates quantity if it already exists.
        /// </summary>
        /// <param name="product">The product to add.</param>
        /// <param name="quantity">The quantity to add (default: 1).</param>
        /// <param name="variant">The selected variant, or null for products without variants.</param>
        public void AddItem(Product product, int quantity = 1, ProductVariant? variant = null)
        {
            var items = GetItems();
            var variantId = variant?.Id;
            var existing = items.FirstOrDefault(i => i.ProductId == product.Id && i.ProductVariantId == variantId);
            if (existing is null)
            {
                var imageUrl = product.Images.FirstOrDefault()?.Url ?? "/uploads/products/placeholder.svg";
                var variantDescription = variant != null
                    ? string.Join(", ", variant.Options
                        .OrderBy(o => o.CategoryVariantType?.SortOrder)
                        .Select(o => $"{o.CategoryVariantType?.Name}: {o.Value}"))
                    : null;

                items.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductVariantId = variantId,
                    Name = product.Name,
                    VariantDescription = variantDescription,
                    Price = variant?.Price ?? product.Price,
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

        public void UpdateQuantity(Guid productId, int quantity, Guid? variantId = null)
        {
            var items = GetItems();
            var existing = items.FirstOrDefault(i => i.ProductId == productId && i.ProductVariantId == variantId);
            if (existing is not null)
            {
                existing.Quantity = Math.Max(1, quantity);
                Session.SetObject(CartKey, items);
            }
        }

        public void RemoveItem(Guid productId, Guid? variantId = null)
        {
            var items = GetItems();
            items.RemoveAll(i => i.ProductId == productId && i.ProductVariantId == variantId);
            Session.SetObject(CartKey, items);
        }

        public void Clear()
        {
            Session.Remove(CartKey);
        }

        public decimal GetTotal() => GetItems().Sum(i => i.Price * i.Quantity);
    }
}
