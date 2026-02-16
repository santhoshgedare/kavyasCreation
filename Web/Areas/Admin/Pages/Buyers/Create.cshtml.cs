using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using BuyerCompanyEntity = Core.Entities.BuyerCompany;
using BuyerUserEntity = Core.Entities.BuyerUser;

namespace Web.Areas.Admin.Pages.Buyers
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IBuyerCompanyRepository _buyerCompanyRepository;
        private readonly IBuyerUserRepository _buyerUserRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public CreateModel(
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

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [StringLength(200)]
            public string CompanyName { get; set; } = string.Empty;

            [StringLength(1000)]
            public string? Description { get; set; }

            [Required]
            [EmailAddress]
            public string CompanyEmail { get; set; } = string.Empty;

            [Phone]
            public string? CompanyPhone { get; set; }

            [StringLength(500)]
            public string? Address { get; set; }

            [StringLength(100)]
            public string? City { get; set; }

            [StringLength(100)]
            public string? State { get; set; }

            [StringLength(20)]
            public string? PostalCode { get; set; }

            [StringLength(100)]
            public string? Country { get; set; }

            [StringLength(100)]
            public string? TaxId { get; set; }

            [StringLength(100)]
            public string? RegistrationNumber { get; set; }

            public bool CreateAdminUser { get; set; }

            [StringLength(100)]
            public string? FirstName { get; set; }

            [StringLength(100)]
            public string? LastName { get; set; }

            [EmailAddress]
            public string? Email { get; set; }

            [Phone]
            public string? PhoneNumber { get; set; }

            [StringLength(100)]
            public string? JobTitle { get; set; }

            [StringLength(100)]
            public string? Department { get; set; }

            [StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string? Password { get; set; }

            public bool AutoApprove { get; set; } = true;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input.CreateAdminUser)
            {
                if (string.IsNullOrEmpty(Input.FirstName))
                    ModelState.AddModelError("Input.FirstName", "First Name is required.");
                if (string.IsNullOrEmpty(Input.LastName))
                    ModelState.AddModelError("Input.LastName", "Last Name is required.");
                if (string.IsNullOrEmpty(Input.Email))
                    ModelState.AddModelError("Input.Email", "Email is required.");
                if (string.IsNullOrEmpty(Input.Password))
                    ModelState.AddModelError("Input.Password", "Password is required.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (await _buyerCompanyRepository.CompanyNameExistsAsync(Input.CompanyName))
            {
                ModelState.AddModelError(string.Empty, "A buyer company with this name already exists.");
                return Page();
            }

            var currentAdmin = await _userManager.GetUserAsync(User);

            var buyerCompany = new BuyerCompanyEntity
            {
                Id = Guid.NewGuid(),
                CompanyName = Input.CompanyName,
                Description = Input.Description,
                ContactEmail = Input.CompanyEmail,
                ContactPhone = Input.CompanyPhone,
                Address = Input.Address,
                City = Input.City,
                State = Input.State,
                PostalCode = Input.PostalCode,
                Country = Input.Country,
                TaxId = Input.TaxId,
                RegistrationNumber = Input.RegistrationNumber,
                IsActive = Input.AutoApprove,
                IsApproved = Input.AutoApprove,
                ApprovedAt = Input.AutoApprove ? DateTime.UtcNow : null,
                ApprovedBy = Input.AutoApprove ? currentAdmin?.Id : null,
                CreatedAt = DateTime.UtcNow
            };

            await _buyerCompanyRepository.AddAsync(buyerCompany);

            if (Input.CreateAdminUser && !string.IsNullOrEmpty(Input.Email) && !string.IsNullOrEmpty(Input.Password))
            {
                var existingUser = await _userManager.FindByEmailAsync(Input.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "A user with this email already exists.");
                    return Page();
                }

                var user = new IdentityUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    var buyerUser = new BuyerUserEntity
                    {
                        Id = Guid.NewGuid(),
                        BuyerCompanyId = buyerCompany.Id,
                        UserId = user.Id,
                        FirstName = Input.FirstName!,
                        LastName = Input.LastName!,
                        Email = Input.Email,
                        PhoneNumber = Input.PhoneNumber,
                        JobTitle = Input.JobTitle ?? "Manager",
                        Department = Input.Department,
                        IsAdmin = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _buyerUserRepository.AddAsync(buyerUser);
                    await _userManager.AddToRoleAsync(user, Core.Constants.Roles.BuyerAdmin);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            TempData["StatusMessage"] = $"Buyer company '{Input.CompanyName}' has been created successfully!";
            return RedirectToPage("./Index");
        }
    }
}
