using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Vendor.Pages.Products
{
    [Authorize(Roles = "VendorAdmin,VendorUser")]
    [Area("Vendor")]
    public class IndexModel : PageModel
    {
        private readonly IProductRepository _productRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(
            IProductRepository productRepository,
            IVendorRepository vendorRepository,
            UserManager<IdentityUser> userManager)
        {
            _productRepository = productRepository;
            _vendorRepository = vendorRepository;
            _userManager = userManager;
        }

        public List<Product> Products { get; set; } = new();
        public string CurrentFilter { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(string? filter, string? status)
        {
            if (!string.IsNullOrEmpty(status))
            {
                StatusMessage = status;
            }

            CurrentFilter = filter ?? string.Empty;

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var vendors = await _vendorRepository.GetAllAsync();
            var vendor = vendors.FirstOrDefault(v => v.ContactEmail == user.Email);

            if (vendor == null)
            {
                StatusMessage = "Error: Vendor profile not found. Please contact support.";
                return Page();
            }

            var allProducts = await _productRepository.ListAsync();
            Products = allProducts.Where(p => p.VendorId == vendor.Id).ToList();

            // Apply filters
            if (!string.IsNullOrEmpty(filter))
            {
                Products = filter.ToLower() switch
                {
                    "active" => Products.Where(p => !p.IsDeleted).ToList(),
                    "inactive" => Products.Where(p => p.IsDeleted).ToList(),
                    "lowstock" => Products.Where(p => p.Stock < 10).ToList(),
                    _ => Products
                };
            }

            Products = Products.OrderByDescending(p => p.Id).ToList();

            return Page();
        }
    }
}
