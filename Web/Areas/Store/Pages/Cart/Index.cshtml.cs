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

