using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using BuyerUserEntity = Core.Entities.BuyerUser;

namespace Web.Areas.Buyer.Pages.Users
{
    [Authorize(Roles = "BuyerAdmin")]
    public class CreateModel : PageModel
    {
        private readonly IBuyerUserRepository _buyerUserRepository;
        private readonly IBuyerCompanyRepository _buyerCompanyRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public CreateModel(
            IBuyerUserRepository buyerUserRepository,
            IBuyerCompanyRepository buyerCompanyRepository,
            UserManager<IdentityUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _buyerUserRepository = buyerUserRepository;
            _buyerCompanyRepository = buyerCompanyRepository;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string CompanyName { get; set; } = string.Empty;

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

            [Display(Name = "Department")]
            [StringLength(100)]
            public string? Department { get; set; }

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

            var currentBuyerUser = await _buyerUserRepository.GetByUserIdAsync(currentUser.Id);
            if (currentBuyerUser == null) return NotFound();

            var company = await _buyerCompanyRepository.GetByIdAsync(currentBuyerUser.BuyerCompanyId);
            if (company != null)
            {
                CompanyName = company.CompanyName;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var currentBuyerUser = await _buyerUserRepository.GetByUserIdAsync(currentUser.Id);
            if (currentBuyerUser == null) return NotFound();

            var company = await _buyerCompanyRepository.GetByIdAsync(currentBuyerUser.BuyerCompanyId);
            if (company == null) return NotFound();

            CompanyName = company.CompanyName;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingUser = await _userManager.FindByEmailAsync(Input.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "A user with this email already exists.");
                return Page();
            }

            var newUser = new IdentityUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(newUser, Input.Password);

            if (result.Succeeded)
            {
                var buyerUser = new BuyerUserEntity
                {
                    Id = Guid.NewGuid(),
                    BuyerCompanyId = company.Id,
                    UserId = newUser.Id,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    Email = Input.Email,
                    PhoneNumber = Input.PhoneNumber,
                    JobTitle = Input.JobTitle,
                    Department = Input.Department,
                    IsAdmin = Input.IsAdmin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _buyerUserRepository.AddAsync(buyerUser);
                await _unitOfWork.SaveChangesAsync();

                var role = Input.IsAdmin ? Core.Constants.Roles.BuyerAdmin : Core.Constants.Roles.BuyerUser;
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
