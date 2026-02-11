using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace kavyasCreation.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            public string UserName { get; set; } = string.Empty;
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            Input.UserName = user?.UserName ?? string.Empty;
        }
    }
}

