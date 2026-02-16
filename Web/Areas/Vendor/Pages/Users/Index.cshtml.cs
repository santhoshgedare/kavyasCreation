using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VendorUserEntity = Core.Entities.VendorUser;

namespace Web.Areas.Vendor.Pages.Users
{
    [Authorize(Roles = "VendorAdmin")]
    public class IndexModel : PageModel
    {
        private readonly IVendorUserRepository _vendorUserRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(
            IVendorUserRepository vendorUserRepository,
            IVendorRepository vendorRepository,
            UserManager<IdentityUser> userManager)
        {
            _vendorUserRepository = vendorUserRepository;
            _vendorRepository = vendorRepository;
            _userManager = userManager;
        }

        public List<VendorUserEntity> VendorUsers { get; set; } = new();
        public string VendorName { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;

        public async Task OnGetAsync(string? status)
        {
            if (!string.IsNullOrEmpty(status))
            {
                StatusMessage = status;
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return;

            // Get current vendor user
            var currentVendorUser = await _vendorUserRepository.GetByUserIdAsync(currentUser.Id);
            if (currentVendorUser == null) return;

            // Get vendor
            var vendor = await _vendorRepository.GetByIdAsync(currentVendorUser.VendorId);
            if (vendor != null)
            {
                VendorName = vendor.CompanyName;
                
                // Get all users for this vendor
                VendorUsers = (await _vendorUserRepository.GetByVendorIdAsync(vendor.Id)).ToList();
            }
        }
    }
}
