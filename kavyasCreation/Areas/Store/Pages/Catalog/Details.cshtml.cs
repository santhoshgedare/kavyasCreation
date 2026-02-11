using Core.Entities;
using Core.Interfaces;
using kavyasCreation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace kavyasCreation.Areas.Store.Pages.Catalog
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CartService _cartService;

        public DetailsModel(IUnitOfWork unitOfWork, CartService cartService)
        {
            _unitOfWork = unitOfWork;
            _cartService = cartService;
        }

        public Product? Product { get; private set; }

        public async Task OnGetAsync(Guid id)
        {
            Product = await _unitOfWork.Products.GetByIdAsync(id);
        }

        public async Task<IActionResult> OnPostAddAsync(Guid productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product is null)
            {
                return RedirectToPage("/Catalog/Index", new { area = "Store" });
            }

            _cartService.AddItem(product);
            return RedirectToPage(new { id = productId });
        }
    }
}
