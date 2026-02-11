using Core.Entities;
using Core.Interfaces;
using Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Store.Pages.Checkout
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly CartService _cartService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(CartService cartService, IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            _cartService = cartService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IReadOnlyList<CartItem> Items { get; private set; } = [];
        public decimal Total { get; private set; }

        public void OnGet()
        {
            Items = _cartService.GetItems();
            Total = _cartService.GetTotal();
        }

        public IActionResult OnPostAsync()
        {
            return RedirectToPage("/Payment/Index", new { area = "Store" });
        }
    }
}
