using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Admin.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class ManageModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public ManageModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        public class UserViewModel
        {
            public string UserId { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? UserName { get; set; }
            public string? PhoneNumber { get; set; }
            public bool EmailConfirmed { get; set; }
            public bool LockoutEnabled { get; set; }
            public DateTimeOffset? LockoutEnd { get; set; }
            public int AccessFailedCount { get; set; }
            public IList<string> Roles { get; set; } = [];
            public string? FullName { get; set; }
            public int AddressCount { get; set; }
            public DateTime? CreatedAt { get; set; }
        }

        public IList<UserViewModel> Users { get; private set; } = [];
        public IList<IdentityRole> AvailableRoles { get; private set; } = [];

        [BindProperty]
        public string SelectedUserId { get; set; } = string.Empty;

        [BindProperty]
        public string SelectedRole { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            var allUsers = _userManager.Users.ToList();
            var userProfiles = await _unitOfWork.UserProfiles.ListAllAsync();

            Users = new List<UserViewModel>();

            foreach (var user in allUsers)
            {
                var profile = userProfiles.FirstOrDefault(p => p.UserId == user.Id);
                var roles = await _userManager.GetRolesAsync(user);

                Users.Add(new UserViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd,
                    AccessFailedCount = user.AccessFailedCount,
                    Roles = roles,
                    FullName = profile?.FullName,
                    AddressCount = profile?.Addresses?.Count ?? 0,
                    CreatedAt = profile?.CreatedAt
                });
            }

            AvailableRoles = _roleManager.Roles.ToList();
        }

        public async Task<IActionResult> OnPostAssignRoleAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedUserId) || string.IsNullOrWhiteSpace(SelectedRole))
            {
                TempData["ErrorMessage"] = "Please select both user and role.";
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(SelectedUserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage();
            }

            var result = await _userManager.AddToRoleAsync(user, SelectedRole);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Role '{SelectedRole}' assigned to {user.Email} successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to assign role: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage();
            }

            var result = await _userManager.RemoveFromRoleAsync(user, role);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Role '{role}' removed from {user.Email} successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to remove role.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostLockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage();
            }

            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User {user.Email} has been locked.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to lock user.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUnlockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage();
            }

            var result = await _userManager.SetLockoutEndDateAsync(user, null);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User {user.Email} has been unlocked.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to unlock user.";
            }

            return RedirectToPage();
        }
    }
}
