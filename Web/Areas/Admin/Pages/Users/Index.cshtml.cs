using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Admin.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IReadOnlyList<UserProfile> UserProfiles { get; private set; } = [];

        public async Task OnGetAsync()
        {
            UserProfiles = await _unitOfWork.UserProfiles.ListAllAsync();
        }
    }
}
