using Core.Entities;
using Core.Models;
using Core.Interfaces;
using Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Store.Pages.Catalog
{
    // REMOVED ResponseCache - it was breaking theme toggle
    // Using OutputCache in Program.cs instead with proper vary headers
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CartService _cartService;
        private readonly ILogger<IndexModel> _logger;
        private const int PageSize = 12;

        public IndexModel(IUnitOfWork unitOfWork, CartService cartService, ILogger<IndexModel> logger)
        {
            _unitOfWork = unitOfWork;
            _cartService = cartService;
            _logger = logger;
        }

        public IReadOnlyList<CatalogProductDto> Products { get; private set; } = [];
        public IReadOnlyList<Category> Categories { get; private set; } = [];
        public int CartCount { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPages { get; private set; }
        public int CurrentPage { get; private set; }
        [BindProperty(SupportsGet = true)]
        public Guid? CategoryId { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public async Task OnGetAsync()
        {
            try
            {
                // Prevent caching for authenticated users (cart count is user-specific)
                if (User.Identity?.IsAuthenticated == true)
                {
                    Response.Headers["Cache-Control"] = "private, no-store, no-cache";
                    Response.Headers["Pragma"] = "no-cache";
                    Response.Headers["Expires"] = "0";
                }
                else
                {
                    Response.Headers["Cache-Control"] = "public, max-age=30";
                }

                Response.Headers["Vary"] = "Cookie"; // Vary by theme cookie
                
                if (PageNumber < 1)
                {
                    PageNumber = 1;
                }

                CurrentPage = PageNumber;

                Categories = await _unitOfWork.Categories.ListAsync();
                var (products, totalCount) = await _unitOfWork.Products.SearchCatalogAsync(Search, CategoryId, CurrentPage, PageSize);
                Products = products;
                TotalCount = totalCount;
                TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
                
                // Only get cart count if user is authenticated
                if (User.Identity?.IsAuthenticated == true)
                {
                    CartCount = _cartService.GetItems().Sum(i => i.Quantity);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading catalog page");
                // Set empty collections to prevent view errors
                Products = [];
                Categories = [];
            }
        }

        [Authorize]
        public async Task<IActionResult> OnPostAddAsync(Guid productId)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product is null)
                {
                    TempData["ErrorMessage"] = "Product not found.";
                    return RedirectToPage();
                }

                if (product.AvailableStock < 1)
                {
                    TempData["WarningMessage"] = "Product is out of stock.";
                    return RedirectToPage();
                }

                _cartService.AddItem(product);
                TempData["SuccessMessage"] = $"{product.Name} added to cart.";

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product {ProductId} to cart", productId);
                TempData["ErrorMessage"] = "Unable to add item to cart. Please try again.";
                return RedirectToPage();
            }
        }
    }
}

