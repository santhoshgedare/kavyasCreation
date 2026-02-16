using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BuyerUserEntity = Core.Entities.BuyerUser;

namespace Web.Areas.Buyer.Pages.Users
{
    [Authorize(Roles = "BuyerAdmin")]
    public class IndexModel : PageModel
    {
        private readonly IBuyerUserRepository _buyerUserRepository;
        private readonly IBuyerCompanyRepository _buyerCompanyRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(
            IBuyerUserRepository buyerUserRepository,
            IBuyerCompanyRepository buyerCompanyRepository,
            UserManager<IdentityUser> userManager)
        {
            _buyerUserRepository = buyerUserRepository;
            _buyerCompanyRepository = buyerCompanyRepository;
            _userManager = userManager;
        }

        public List<BuyerUserEntity> BuyerUsers { get; set; } = new();
        public string CompanyName { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;

        public async Task OnGetAsync(string? status)
        {
            if (!string.IsNullOrEmpty(status))
            {
                StatusMessage = status;
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return;

            var currentBuyerUser = await _buyerUserRepository.GetByUserIdAsync(currentUser.Id);
            if (currentBuyerUser == null) return;

            var company = await _buyerCompanyRepository.GetByIdAsync(currentBuyerUser.BuyerCompanyId);
            if (company != null)
            {
                CompanyName = company.CompanyName;
                BuyerUsers = (await _buyerUserRepository.GetByBuyerCompanyIdAsync(company.Id)).ToList();
            }
        }
    }
}
