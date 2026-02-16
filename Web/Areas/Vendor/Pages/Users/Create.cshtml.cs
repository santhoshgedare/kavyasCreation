using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using VendorUserEntity = Core.Entities.VendorUser;

namespace Web.Areas.Vendor.Pages.Users
{
    [Authorize(Roles = "VendorAdmin")]
    public class CreateModel : PageModel
    {
        private readonly IVendorUserRepository _vendorUserRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public CreateModel(
            IVendorUserRepository vendorUserRepository,
            IVendorRepository vendorRepository,
            UserManager<IdentityUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _vendorUserRepository = vendorUserRepository;
            _vendorRepository = vendorRepository;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string VendorName { get; set; } = string.Empty;

        public class InputModel
        {
            [Required]
            [Display(Name = "First Name")]
            [StringLength(100)]
            public string FirstName { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Last Name")]
            [StringLength(100)]
            public string LastName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Phone]
            [Display(Name = "Phone Number")]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Job Title")]
            [StringLength(100)]
            public string? JobTitle { get; set; }

            [Display(Name = "Is Admin")]
            public bool IsAdmin { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var currentVendorUser = await _vendorUserRepository.GetByUserIdAsync(currentUser.Id);
            if (currentVendorUser == null) return NotFound();

            var vendor = await _vendorRepository.GetByIdAsync(currentVendorUser.VendorId);
            if (vendor != null)
            {
                VendorName = vendor.CompanyName;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var currentVendorUser = await _vendorUserRepository.GetByUserIdAsync(currentUser.Id);
            if (currentVendorUser == null) return NotFound();

            var vendor = await _vendorRepository.GetByIdAsync(currentVendorUser.VendorId);
            if (vendor == null) return NotFound();

            VendorName = vendor.CompanyName;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(Input.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "A user with this email already exists.");
                return Page();
            }

            // Create Identity user
            var newUser = new IdentityUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                EmailConfirmed = true // Company admin is creating, so confirm email
            };

            var result = await _userManager.CreateAsync(newUser, Input.Password);

            if (result.Succeeded)
            {
                // Create VendorUser
                var vendorUser = new VendorUserEntity
                {
                    Id = Guid.NewGuid(),
                    VendorId = vendor.Id,
                    UserId = newUser.Id,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    Email = Input.Email,
                    PhoneNumber = Input.PhoneNumber,
                    JobTitle = Input.JobTitle,
                    IsAdmin = Input.IsAdmin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _vendorUserRepository.AddAsync(vendorUser);
                await _unitOfWork.SaveChangesAsync();

                // Assign role
                var role = Input.IsAdmin ? Core.Constants.Roles.VendorAdmin : Core.Constants.Roles.VendorUser;
                await _userManager.AddToRoleAsync(newUser, role);

                return RedirectToPage("./Index", new { status = $"User {Input.FirstName} {Input.LastName} has been added successfully!" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
