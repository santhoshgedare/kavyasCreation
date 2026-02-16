using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Vendor.Pages.Dashboard
{
    [Authorize(Roles = "VendorAdmin,VendorUser")]
    public class IndexModel : PageModel
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            IVendorRepository vendorRepository,
            UserManager<IdentityUser> userManager)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _vendorRepository = vendorRepository;
            _userManager = userManager;
        }

        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int LowStockCount { get; set; }
        public string VendorName { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            // Get vendor info
            var vendors = await _vendorRepository.GetAllAsync();
            var vendor = vendors.FirstOrDefault(v => v.ContactEmail == user.Email);
            
            if (vendor != null)
            {
                VendorName = vendor.CompanyName;
                
                // Get all products for this vendor
                var allProducts = await _productRepository.ListAsync();
                var vendorProducts = allProducts.Where(p => p.VendorId == vendor.Id).ToList();
                
                TotalProducts = vendorProducts.Count;
                ActiveProducts = vendorProducts.Count(p => !p.IsDeleted);
                LowStockCount = vendorProducts.Count(p => p.Stock < 10);

                // Note: Order statistics would require additional repository methods
                // For now, set to 0 as placeholder
                TotalOrders = 0;
                PendingOrders = 0;
                TotalRevenue = 0;
                MonthlyRevenue = 0;
            }
        }
    }
}
