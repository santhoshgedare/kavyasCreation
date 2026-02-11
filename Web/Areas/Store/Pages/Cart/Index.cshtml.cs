using Core.Entities;
using Core.Interfaces;
using Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Store.Pages.Cart
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly CartService _cartService;
        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(CartService cartService, IUnitOfWork unitOfWork)
        {
            _cartService = cartService;
            _unitOfWork = unitOfWork;
        }

        public IReadOnlyList<CartItem> Items { get; private set; } = [];
        public decimal Total { get; private set; }
        public Dictionary<Guid, int> AvailableStock { get; private set; } = new();

        public async Task OnGetAsync()
        {
            Items = _cartService.GetItems();
            Total = _cartService.GetTotal();
            
            // Check available stock for each item
            foreach (var item in Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product is not null)
                {
                    AvailableStock[item.ProductId] = product.AvailableStock;
                }
            }
        }

        public IActionResult OnPostUpdate(Guid productId, int quantity)
        {
            _cartService.UpdateQuantity(productId, quantity);
            return RedirectToPage();
        }

        public IActionResult OnPostRemove(Guid productId)
        {
            _cartService.RemoveItem(productId);
            return RedirectToPage();
        }
    }
}
