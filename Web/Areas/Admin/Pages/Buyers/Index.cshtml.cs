using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BuyerCompanyEntity = Core.Entities.BuyerCompany;

namespace Web.Areas.Admin.Pages.Buyers
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IBuyerCompanyRepository _buyerCompanyRepository;

        public IndexModel(IBuyerCompanyRepository buyerCompanyRepository)
        {
            _buyerCompanyRepository = buyerCompanyRepository;
        }

        public List<BuyerCompanyEntity> BuyerCompanies { get; set; } = new();
        public string StatusMessage { get; set; } = string.Empty;

        public async Task OnGetAsync(string? status)
        {
            if (!string.IsNullOrEmpty(status))
            {
                StatusMessage = status;
            }

            var allBuyers = await _buyerCompanyRepository.GetAllAsync();
            BuyerCompanies = allBuyers.OrderByDescending(b => b.CreatedAt).ToList();
        }
    }
}
