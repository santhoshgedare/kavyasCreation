using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BuyerCompanyEntity = Core.Entities.BuyerCompany;
using BuyerUserEntity = Core.Entities.BuyerUser;

namespace Web.Areas.Admin.Pages.Buyers
{
    [Authorize(Roles = "Admin")]
    public class PendingModel : PageModel
    {
        private readonly IBuyerCompanyRepository _buyerCompanyRepository;
        private readonly IBuyerUserRepository _buyerUserRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public PendingModel(
            IBuyerCompanyRepository buyerCompanyRepository,
            IBuyerUserRepository buyerUserRepository,
            UserManager<IdentityUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _buyerCompanyRepository = buyerCompanyRepository;
            _buyerUserRepository = buyerUserRepository;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public List<BuyerWithUser> PendingBuyers { get; set; } = new();
        public string StatusMessage { get; set; } = string.Empty;

        public class BuyerWithUser
        {
            public BuyerCompanyEntity BuyerCompany { get; set; } = null!;
            public BuyerUserEntity? AdminUser { get; set; }
        }

        public async Task OnGetAsync(string? status)
        {
            if (!string.IsNullOrEmpty(status))
            {
                StatusMessage = status;
            }

            var allBuyers = await _buyerCompanyRepository.GetAllAsync();
            var pendingBuyers = allBuyers.Where(b => !b.IsApproved).ToList();

            foreach (var buyer in pendingBuyers)
            {
                var buyerUsers = await _buyerUserRepository.GetByBuyerCompanyIdAsync(buyer.Id);
                var adminUser = buyerUsers.FirstOrDefault(bu => bu.IsAdmin);

                PendingBuyers.Add(new BuyerWithUser
                {
                    BuyerCompany = buyer,
                    AdminUser = adminUser
                });
            }
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid id)
        {
            var buyer = await _buyerCompanyRepository.GetByIdAsync(id);
            if (buyer == null)
            {
                return NotFound();
            }

            var admin = await _userManager.GetUserAsync(User);
            if (admin == null)
            {
                return Unauthorized();
            }

            // Approve buyer company
            buyer.IsApproved = true;
            buyer.IsActive = true;
            buyer.ApprovedAt = DateTime.UtcNow;
            buyer.ApprovedBy = admin.Id;

            _buyerCompanyRepository.Update(buyer);

            // Activate all buyer users
            var buyerUsers = await _buyerUserRepository.GetByBuyerCompanyIdAsync(id);
            foreach (var buyerUser in buyerUsers)
            {
                buyerUser.IsActive = true;
                _buyerUserRepository.Update(buyerUser);
            }

            await _unitOfWork.SaveChangesAsync();

            return RedirectToPage(new { status = $"Buyer company '{buyer.CompanyName}' has been approved successfully!" });
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid id)
        {
            var buyer = await _buyerCompanyRepository.GetByIdAsync(id);
            if (buyer == null)
            {
                return NotFound();
            }

            // Soft delete the buyer company
            buyer.IsDeleted = true;
            buyer.DeletedAt = DateTime.UtcNow;

            _buyerCompanyRepository.Update(buyer);

            // Deactivate buyer users
            var buyerUsers = await _buyerUserRepository.GetByBuyerCompanyIdAsync(id);
            foreach (var buyerUser in buyerUsers)
            {
                buyerUser.IsDeleted = true;
                buyerUser.DeletedAt = DateTime.UtcNow;
                _buyerUserRepository.Update(buyerUser);
            }

            await _unitOfWork.SaveChangesAsync();

            return RedirectToPage(new { status = "Buyer company application has been rejected." });
        }
    }
}
