using Core.Entities;
using Core.Interfaces;
using Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Store.Pages.Catalog
{
    public class DetailsModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CartService _cartService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IUnitOfWork unitOfWork, CartService cartService, ILogger<DetailsModel> logger)
        {
            _unitOfWork = unitOfWork;
            _cartService = cartService;
            _logger = logger;
        }

        public Product? Product { get; private set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    _logger.LogWarning("Invalid product ID requested");
                    TempData["Error"] = "Invalid product ID";
                    return RedirectToPage("/Catalog/Index", new { area = "Store" });
                }

                Product = await _unitOfWork.Products.GetByIdAsync(id);
                
                if (Product is null)
                {
                    _logger.LogWarning("Product {ProductId} not found", id);
                    TempData["Error"] = "Product not found";
                    return RedirectToPage("/Catalog/Index", new { area = "Store" });
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product details for {ProductId}", id);
                TempData["Error"] = "Unable to load product details. Please try again.";
                return RedirectToPage("/Catalog/Index", new { area = "Store" });
            }
        }

        [Authorize]
        public async Task<IActionResult> OnPostAddAsync(Guid productId)
        {
            try
            {
                if (productId == Guid.Empty)
                {
                    _logger.LogWarning("Invalid product ID in add to cart");
                    TempData["Error"] = "Invalid product";
                    return RedirectToPage("/Catalog/Index", new { area = "Store" });
                }

                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product is null)
                {
                    _logger.LogWarning("Product {ProductId} not found when adding to cart", productId);
                    TempData["Error"] = "Product not found";
                    return RedirectToPage("/Catalog/Index", new { area = "Store" });
                }

                if (product.AvailableStock < 1)
                {
                    _logger.LogInformation("Attempted to add out-of-stock product {ProductId} to cart", productId);
                    TempData["Error"] = $"{product.Name} is currently out of stock";
                    return RedirectToPage(new { id = productId });
                }

                _cartService.AddItem(product);
                TempData["Success"] = $"{product.Name} added to cart successfully";
                
                _logger.LogInformation("Product {ProductId} added to cart for user {UserId}", productId, User.Identity?.Name);
                
                return RedirectToPage(new { id = productId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product {ProductId} to cart", productId);
                TempData["Error"] = "Unable to add item to cart. Please try again.";
                return RedirectToPage(new { id = productId });
            }
        }
    }
}

