using Core.Entities;
using Core.Interfaces;
using Core.Constants;
using Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Web.Areas.Store.Pages.Checkout
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly CartService _cartService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _db;

        public IndexModel(CartService cartService, IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, AppDbContext db)
        {
            _cartService = cartService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _db = db;
        }

        public IReadOnlyList<CartItem> Items { get; private set; } = [];
        public decimal Total { get; private set; }
        public decimal ShippingCharge { get; private set; }
        public decimal GrandTotal { get; private set; }
        public UserAddress? ShippingAddress { get; private set; }

        public async Task OnGetAsync()
        {
            Items = _cartService.GetItems();
            Total = _cartService.GetTotal();
            ShippingCharge = Items.Any() ? AppConstants.StandardShippingCharge : 0m;
            GrandTotal = Total + ShippingCharge;

            if (Items.Any())
            {
                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    ShippingAddress = await GetShippingAddressAsync(userId);
                }
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var items = _cartService.GetItems();
            if (!items.Any())
            {
                TempData["WarningMessage"] = "Your cart is empty.";
                return RedirectToPage("/Cart/Index", new { area = "Store" });
            }

            var productIds = items.Select(i => i.ProductId).ToList();
            var stock = await _db.Products
                .Where(p => productIds.Contains(p.Id))
                .Select(p => new { p.Id, p.AvailableStock })
                .AsNoTracking()
                .ToDictionaryAsync(p => p.Id, p => p.AvailableStock);

            foreach (var item in items)
            {
                if (!stock.TryGetValue(item.ProductId, out var available) || available <= 0)
                {
                    TempData["WarningMessage"] = $"{item.Name} is out of stock. Please update your cart.";
                    return RedirectToPage("/Cart/Index", new { area = "Store" });
                }

                if (item.Quantity > available)
                {
                    TempData["WarningMessage"] = $"Only {available} of {item.Name} available. Please update your cart.";
                    return RedirectToPage("/Cart/Index", new { area = "Store" });
                }
            }

            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                TempData["ErrorMessage"] = "Unable to verify your account. Please sign in again.";
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var shippingAddress = await GetShippingAddressAsync(currentUserId);
            if (!IsValidShippingAddress(shippingAddress))
            {
                TempData["WarningMessage"] = "Please add a valid shipping address before checkout.";
                return RedirectToPage("/Profile/Addresses", new { area = "Account" });
            }

            return RedirectToPage("/Payment/Index", new { area = "Store" });
        }

        private async Task<UserAddress?> GetShippingAddressAsync(string userId)
        {
            var profileId = await _db.UserProfiles
                .Where(p => p.UserId == userId)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            if (profileId == Guid.Empty)
            {
                return null;
            }

            return await _db.UserAddresses
                .Where(a => a.UserProfileId == profileId && !a.IsDeleted)
                .OrderByDescending(a => a.IsShippingDefault)
                .ThenByDescending(a => a.IsPrimary)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        private static bool IsValidShippingAddress(UserAddress? address)
        {
            return address is not null
                && !string.IsNullOrWhiteSpace(address.AddressLine1)
                && !string.IsNullOrWhiteSpace(address.City)
                && !string.IsNullOrWhiteSpace(address.State)
                && !string.IsNullOrWhiteSpace(address.PostalCode)
                && !string.IsNullOrWhiteSpace(address.Country);
        }
    }
}
