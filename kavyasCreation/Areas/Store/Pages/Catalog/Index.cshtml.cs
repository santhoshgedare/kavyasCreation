using Core.Entities;
using Core.Interfaces;
using kavyasCreation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace kavyasCreation.Areas.Store.Pages.Catalog
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CartService _cartService;

        public IndexModel(IUnitOfWork unitOfWork, CartService cartService)
        {
            _unitOfWork = unitOfWork;
            _cartService = cartService;
        }

        public IReadOnlyList<Product> Products { get; private set; } = [];
        public IReadOnlyList<Category> Categories { get; private set; } = [];
        public int CartCount { get; private set; }
        [BindProperty(SupportsGet = true)]
        public Guid? CategoryId { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _unitOfWork.Categories.ListAsync();
            Products = await _unitOfWork.Products.ListByCategoryAsync(CategoryId, Search);
            CartCount = _cartService.GetItems().Sum(i => i.Quantity);
        }

        public async Task<IActionResult> OnPostAddAsync(Guid productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product is null)
            {
                return RedirectToPage();
            }

            _cartService.AddItem(product);
            return RedirectToPage();
        }
    }
}
