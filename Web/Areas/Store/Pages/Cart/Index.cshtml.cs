using Core.Entities;
using Core.Interfaces;
using Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Infra.Data;

namespace Web.Areas.Store.Pages.Cart
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly CartService _cartService;
        private readonly AppDbContext _db;

        public IndexModel(CartService cartService, AppDbContext db)
        {
            _cartService = cartService;
            _db = db;
        }

        public IReadOnlyList<CartItem> Items { get; private set; } = [];
        public decimal Total { get; private set; }
        public Dictionary<Guid, int> AvailableStock { get; private set; } = new();

        public async Task OnGetAsync()
        {
            Items = _cartService.GetItems();
            Total = _cartService.GetTotal();
            
            // OPTIMIZED: Single query to get all stock info instead of N queries
            if (Items.Any())
            {
                var productIds = Items.Select(i => i.ProductId).ToList();
                
                AvailableStock = await _db.Products
                    .Where(p => productIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.AvailableStock })
                    .AsNoTracking()
                    .ToDictionaryAsync(p => p.Id, p => p.AvailableStock);
            }
        }

        public async Task<IActionResult> OnPostUpdateAsync(Guid productId, int quantity)
        {
            if (quantity < 1)
            {
                TempData["WarningMessage"] = "Quantity must be at least 1.";
                return RedirectToPage();
            }

            var available = await _db.Products
                .Where(p => p.Id == productId)
                .Select(p => p.AvailableStock)
                .FirstOrDefaultAsync();

            if (available <= 0)
            {
                TempData["WarningMessage"] = "This item is currently out of stock.";
                _cartService.RemoveItem(productId);
                return RedirectToPage();
            }

            if (quantity > available)
            {
                _cartService.UpdateQuantity(productId, available);
                TempData["WarningMessage"] = $"Only {available} available. Quantity updated.";
                return RedirectToPage();
            }

            _cartService.UpdateQuantity(productId, quantity);
            TempData["SuccessMessage"] = "Cart updated.";
            return RedirectToPage();
        }

        public IActionResult OnPostRemove(Guid productId)
        {
            _cartService.RemoveItem(productId);
            TempData["InfoMessage"] = "Item removed from cart.";
            return RedirectToPage();
        }
    }
}

