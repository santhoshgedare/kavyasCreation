using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VendorEntity = Core.Entities.Vendor;

namespace Web.Areas.Admin.Pages.Vendors
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IVendorRepository _vendorRepository;

        public IndexModel(IVendorRepository vendorRepository)
        {
            _vendorRepository = vendorRepository;
        }

        public List<VendorEntity> Vendors { get; set; } = new();
        public string StatusMessage { get; set; } = string.Empty;

        public async Task OnGetAsync(string? status)
        {
            if (!string.IsNullOrEmpty(status))
            {
                StatusMessage = status;
            }

            var allVendors = await _vendorRepository.GetAllAsync();
            Vendors = allVendors.OrderByDescending(v => v.CreatedAt).ToList();
        }
    }
}
