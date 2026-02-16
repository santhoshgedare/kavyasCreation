using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VendorEntity = Core.Entities.Vendor;
using VendorUserEntity = Core.Entities.VendorUser;

namespace Web.Areas.Admin.Pages.Vendors
{
    [Authorize(Roles = "Admin")]
    public class PendingModel : PageModel
    {
        private readonly IVendorRepository _vendorRepository;
        private readonly IVendorUserRepository _vendorUserRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public PendingModel(
            IVendorRepository vendorRepository,
            IVendorUserRepository vendorUserRepository,
            UserManager<IdentityUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _vendorRepository = vendorRepository;
            _vendorUserRepository = vendorUserRepository;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public List<VendorWithUser> PendingVendors { get; set; } = new();
        public string StatusMessage { get; set; } = string.Empty;

        public class VendorWithUser
        {
            public VendorEntity Vendor { get; set; } = null!;
            public VendorUserEntity? AdminUser { get; set; }
        }

        public async Task OnGetAsync(string? status)
        {
            if (!string.IsNullOrEmpty(status))
            {
                StatusMessage = status;
            }

            var allVendors = await _vendorRepository.GetAllAsync();
            var pendingVendors = allVendors.Where(v => !v.IsApproved).ToList();

            foreach (var vendor in pendingVendors)
            {
                var vendorUsers = await _vendorUserRepository.GetByVendorIdAsync(vendor.Id);
                var adminUser = vendorUsers.FirstOrDefault(vu => vu.IsAdmin);

                PendingVendors.Add(new VendorWithUser
                {
                    Vendor = vendor,
                    AdminUser = adminUser
                });
            }
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid id)
        {
            var vendor = await _vendorRepository.GetByIdAsync(id);
            if (vendor == null)
            {
                return NotFound();
            }

            var admin = await _userManager.GetUserAsync(User);
            if (admin == null)
            {
                return Unauthorized();
            }

            // Approve vendor
            vendor.IsApproved = true;
            vendor.IsActive = true;
            vendor.ApprovedAt = DateTime.UtcNow;
            vendor.ApprovedBy = admin.Id;

            _vendorRepository.Update(vendor);

            // Activate all vendor users
            var vendorUsers = await _vendorUserRepository.GetByVendorIdAsync(id);
            foreach (var vendorUser in vendorUsers)
            {
                vendorUser.IsActive = true;
                _vendorUserRepository.Update(vendorUser);
            }

            await _unitOfWork.SaveChangesAsync();

            // TODO: Send email notification to vendor

            return RedirectToPage(new { status = $"Vendor '{vendor.CompanyName}' has been approved successfully!" });
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid id)
        {
            var vendor = await _vendorRepository.GetByIdAsync(id);
            if (vendor == null)
            {
                return NotFound();
            }

            // Soft delete the vendor
            vendor.IsDeleted = true;
            vendor.DeletedAt = DateTime.UtcNow;

            _vendorRepository.Update(vendor);

            // Deactivate vendor users
            var vendorUsers = await _vendorUserRepository.GetByVendorIdAsync(id);
            foreach (var vendorUser in vendorUsers)
            {
                vendorUser.IsDeleted = true;
                vendorUser.DeletedAt = DateTime.UtcNow;
                _vendorUserRepository.Update(vendorUser);
            }

            await _unitOfWork.SaveChangesAsync();

            // TODO: Send rejection email to vendor

            return RedirectToPage(new { status = $"Vendor application has been rejected." });
        }
    }
}
