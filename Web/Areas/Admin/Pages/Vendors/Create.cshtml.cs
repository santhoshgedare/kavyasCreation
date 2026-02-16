using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using VendorEntity = Core.Entities.Vendor;
using VendorUserEntity = Core.Entities.VendorUser;

namespace Web.Areas.Admin.Pages.Vendors
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IVendorRepository _vendorRepository;
        private readonly IVendorUserRepository _vendorUserRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public CreateModel(
            IVendorRepository vendorRepository,
            IVendorUserRepository vendorUserRepository,
            UserManager<IdentityUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _vendorRepository = vendorRepository;
            _vendorUserRepository = vendorUserRepository;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            // Company Info
            [Required]
            [Display(Name = "Company Name")]
            [StringLength(200)]
            public string CompanyName { get; set; } = string.Empty;

            [Display(Name = "Description")]
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

            // Admin User (Optional - can be added later)
            [Display(Name = "Create Admin User?")]
            public bool CreateAdminUser { get; set; }

            [Display(Name = "First Name")]
            [StringLength(100)]
            public string? FirstName { get; set; }

            [Display(Name = "Last Name")]
            [StringLength(100)]
            public string? LastName { get; set; }

            [EmailAddress]
            [Display(Name = "Email")]
            public string? Email { get; set; }

            [Phone]
            [Display(Name = "Phone Number")]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Job Title")]
            [StringLength(100)]
            public string? JobTitle { get; set; }

            [StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string? Password { get; set; }

            [Display(Name = "Auto-Approve")]
            public bool AutoApprove { get; set; } = true;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Custom validation for admin user creation
            if (Input.CreateAdminUser)
            {
                if (string.IsNullOrEmpty(Input.FirstName))
                    ModelState.AddModelError("Input.FirstName", "First Name is required when creating admin user.");
                if (string.IsNullOrEmpty(Input.LastName))
                    ModelState.AddModelError("Input.LastName", "Last Name is required when creating admin user.");
                if (string.IsNullOrEmpty(Input.Email))
                    ModelState.AddModelError("Input.Email", "Email is required when creating admin user.");
                if (string.IsNullOrEmpty(Input.Password))
                    ModelState.AddModelError("Input.Password", "Password is required when creating admin user.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if company name exists
            if (await _vendorRepository.CompanyNameExistsAsync(Input.CompanyName))
            {
                ModelState.AddModelError(string.Empty, "A vendor with this company name already exists.");
                return Page();
            }

            var currentAdmin = await _userManager.GetUserAsync(User);

            // Create Vendor Company
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
                IsActive = Input.AutoApprove,
                IsApproved = Input.AutoApprove,
                ApprovedAt = Input.AutoApprove ? DateTime.UtcNow : null,
                ApprovedBy = Input.AutoApprove ? currentAdmin?.Id : null,
                CreatedAt = DateTime.UtcNow
            };

            await _vendorRepository.AddAsync(vendor);

            // Create admin user if requested
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
                    EmailConfirmed = true // Admin-created, so confirm email
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    var vendorUser = new VendorUserEntity
                    {
                        Id = Guid.NewGuid(),
                        VendorId = vendor.Id,
                        UserId = user.Id,
                        FirstName = Input.FirstName!,
                        LastName = Input.LastName!,
                        Email = Input.Email,
                        PhoneNumber = Input.PhoneNumber,
                        JobTitle = Input.JobTitle ?? "Owner",
                        IsAdmin = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _vendorUserRepository.AddAsync(vendorUser);
                    await _userManager.AddToRoleAsync(user, Core.Constants.Roles.VendorAdmin);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            TempData["StatusMessage"] = $"Vendor company '{Input.CompanyName}' has been created successfully!";
            return RedirectToPage("./Index");
        }
    }
}
