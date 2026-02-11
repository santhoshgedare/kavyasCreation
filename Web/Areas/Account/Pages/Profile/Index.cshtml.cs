using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Web.Areas.Account.Pages.Profile
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

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        [BindProperty]
        public AddressInputModel? AddressInput { get; set; }

        public bool HasProfile { get; private set; }
        public List<UserAddress> Addresses { get; private set; } = [];

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
            [StringLength(256)]
            public string Email { get; set; } = string.Empty;

            [EmailAddress]
            [Display(Name = "Alternate Email")]
            [StringLength(256)]
            public string? AlternateEmail { get; set; }

            [Phone]
            [Display(Name = "Phone Number")]
            [StringLength(20)]
            public string? PhoneNumber { get; set; }

            [Phone]
            [Display(Name = "Alternate Phone")]
            [StringLength(20)]
            public string? AlternatePhoneNumber { get; set; }

            [Display(Name = "Date of Birth")]
            [DataType(DataType.Date)]
            public DateTime? DateOfBirth { get; set; }

            [Display(Name = "Gender")]
            public string? Gender { get; set; }
        }

        public class AddressInputModel
        {
            [Display(Name = "Add My First Address")]
            public bool AddAddress { get; set; }

            [Display(Name = "Label")]
            [StringLength(200)]
            public string Label { get; set; } = "Home";

            [Display(Name = "Address Line 1")]
            [StringLength(500)]
            public string? AddressLine1 { get; set; }

            [Display(Name = "Address Line 2")]
            [StringLength(500)]
            public string? AddressLine2 { get; set; }

            [Display(Name = "City")]
            [StringLength(100)]
            public string? City { get; set; }

            [Display(Name = "State/Province")]
            [StringLength(100)]
            public string? State { get; set; }

            [Display(Name = "Postal Code")]
            [StringLength(20)]
            public string? PostalCode { get; set; }

            [Display(Name = "Country")]
            [StringLength(100)]
            public string Country { get; set; } = "USA";
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var profile = await _unitOfWork.UserProfiles.GetByUserIdAsync(userId);
            
            if (profile != null)
            {
                HasProfile = true;
                Addresses = profile.Addresses.ToList();
                Input = new InputModel
                {
                    FirstName = profile.FirstName,
                    LastName = profile.LastName,
                    Email = profile.Email,
                    AlternateEmail = profile.AlternateEmail,
                    PhoneNumber = profile.PhoneNumber,
                    AlternatePhoneNumber = profile.AlternatePhoneNumber,
                    DateOfBirth = profile.DateOfBirth,
                    Gender = profile.Gender
                };
            }
            else
            {
                HasProfile = false;
                var identityUser = await _userManager.GetUserAsync(User);
                Input = new InputModel
                {
                    Email = identityUser?.Email ?? string.Empty,
                    PhoneNumber = identityUser?.PhoneNumber,
                    FirstName = string.Empty,
                    LastName = string.Empty
                };
                
                // Initialize address input for new profiles
                AddressInput = new AddressInputModel();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var existingProfile = await _unitOfWork.UserProfiles.GetByUserIdAsync(userId);

            if (existingProfile != null)
            {
                // Update existing profile
                existingProfile.FirstName = Input.FirstName;
                existingProfile.LastName = Input.LastName;
                existingProfile.Email = Input.Email;
                existingProfile.AlternateEmail = Input.AlternateEmail;
                existingProfile.PhoneNumber = Input.PhoneNumber;
                existingProfile.AlternatePhoneNumber = Input.AlternatePhoneNumber;
                existingProfile.DateOfBirth = Input.DateOfBirth;
                existingProfile.Gender = Input.Gender;

                await _unitOfWork.UserProfiles.UpdateAsync(existingProfile);
                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
            else
            {
                // Create new profile with optional address
                var newProfile = new UserProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    Email = Input.Email,
                    AlternateEmail = Input.AlternateEmail,
                    PhoneNumber = Input.PhoneNumber,
                    AlternatePhoneNumber = Input.AlternatePhoneNumber,
                    DateOfBirth = Input.DateOfBirth,
                    Gender = Input.Gender
                };

                // Add first address if user opted to
                if (AddressInput?.AddAddress == true && 
                    !string.IsNullOrWhiteSpace(AddressInput.AddressLine1) &&
                    !string.IsNullOrWhiteSpace(AddressInput.City) &&
                    !string.IsNullOrWhiteSpace(AddressInput.State) &&
                    !string.IsNullOrWhiteSpace(AddressInput.PostalCode))
                {
                    var firstAddress = new UserAddress
                    {
                        Id = Guid.NewGuid(),
                        UserProfileId = newProfile.Id,
                        Label = AddressInput.Label,
                        AddressLine1 = AddressInput.AddressLine1,
                        AddressLine2 = AddressInput.AddressLine2,
                        City = AddressInput.City,
                        State = AddressInput.State,
                        PostalCode = AddressInput.PostalCode,
                        Country = AddressInput.Country,
                        PhoneNumber = Input.PhoneNumber, // Use profile phone
                        IsPrimary = true, // First address is primary
                        IsShippingDefault = true // First address is shipping default
                    };

                    newProfile.Addresses.Add(firstAddress);
                    TempData["SuccessMessage"] = "Profile and address created successfully!";
                }
                else
                {
                    TempData["SuccessMessage"] = "Profile created successfully! You can add addresses later.";
                }

                await _unitOfWork.UserProfiles.AddAsync(newProfile);
            }

            return RedirectToPage();
        }
    }
}


