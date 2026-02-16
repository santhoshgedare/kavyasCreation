using Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using VendorEntity = Core.Entities.Vendor;
using VendorUserEntity = Core.Entities.VendorUser;

namespace Web.Areas.Identity.Pages.Account
{
    public class RegisterVendorModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IVendorRepository _vendorRepository;
        private readonly IVendorUserRepository _vendorUserRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterVendorModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IVendorRepository vendorRepository,
            IVendorUserRepository vendorUserRepository,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _vendorRepository = vendorRepository;
            _vendorUserRepository = vendorUserRepository;
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string ReturnUrl { get; set; } = string.Empty;

        public class InputModel
        {
            // Company Info
            [Required]
            [Display(Name = "Company Name")]
            [StringLength(200)]
            public string CompanyName { get; set; } = string.Empty;

            [Display(Name = "Company Description")]
            [StringLength(1000)]
            public string? Description { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Company Email")]
            public string CompanyEmail { get; set; } = string.Empty;

            [Phone]
            [Display(Name = "Company Phone")]
            public string? CompanyPhone { get; set; }

            [Display(Name = "Address")]
            [StringLength(500)]
            public string? Address { get; set; }

            [Display(Name = "City")]
            [StringLength(100)]
            public string? City { get; set; }

            [Display(Name = "State")]
            [StringLength(100)]
            public string? State { get; set; }

            [Display(Name = "Postal Code")]
            [StringLength(20)]
            public string? PostalCode { get; set; }

            [Display(Name = "Country")]
            [StringLength(100)]
            public string? Country { get; set; }

            [Display(Name = "Tax ID / GST Number")]
            [StringLength(100)]
            public string? TaxId { get; set; }

            [Display(Name = "Registration Number")]
            [StringLength(100)]
            public string? RegistrationNumber { get; set; }

            // Admin User Info
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

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                // Check if company name already exists
                if (await _vendorRepository.CompanyNameExistsAsync(Input.CompanyName))
                {
                    ModelState.AddModelError(string.Empty, "A vendor with this company name already exists.");
                    return Page();
                }

                // Create Identity user
                var user = new IdentityUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    EmailConfirmed = false // Require email confirmation
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    // Create Vendor (Company)
                    var vendor = new VendorEntity
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
                        IsActive = false, // Require admin approval
                        IsApproved = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _vendorRepository.AddAsync(vendor);

                    // Create VendorUser (Admin user for this vendor)
                    var vendorUser = new VendorUserEntity
                    {
                        Id = Guid.NewGuid(),
                        VendorId = vendor.Id,
                        UserId = user.Id,
                        FirstName = Input.FirstName,
                        LastName = Input.LastName,
                        Email = Input.Email,
                        PhoneNumber = Input.PhoneNumber,
                        JobTitle = Input.JobTitle ?? "Owner",
                        IsAdmin = true, // This is the vendor admin
                        IsActive = false, // Will be activated when vendor is approved
                        CreatedAt = DateTime.UtcNow
                    };

                    await _vendorUserRepository.AddAsync(vendorUser);
                    await _unitOfWork.SaveChangesAsync();

                    // Assign "VendorAdmin" role (but user can't access until approved)
                    await _userManager.AddToRoleAsync(user, Core.Constants.Roles.VendorAdmin);

                    TempData["StatusMessage"] = "Registration successful! Your application is pending admin approval. You will receive an email once approved.";
                    return RedirectToPage("./RegisterConfirmation", new { email = Input.Email, returnUrl });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}
