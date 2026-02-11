using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace kavyasCreation.Areas.Store.Pages.Orders
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public List<Order> Orders { get; private set; } = [];

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            Orders = (await _unitOfWork.Orders.ListByUserAsync(userId)).ToList();
        }
    }
}
