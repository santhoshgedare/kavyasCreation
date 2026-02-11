using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace kavyasCreation.Areas.Account.Pages.Profile
{
    [Authorize]
    public class AddressesModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;

        public AddressesModel(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IReadOnlyList<UserAddress> Addresses { get; private set; } = [];
        public Guid UserProfileId { get; private set; }

        [BindProperty]
        public AddressInputModel Input { get; set; } = default!;

        public class AddressInputModel
        {
            public Guid? Id { get; set; }

            [Required]
            [Display(Name = "Label")]
            [StringLength(200)]
            public string Label { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Address Line 1")]
            [StringLength(500)]
            public string AddressLine1 { get; set; } = string.Empty;

            [Display(Name = "Address Line 2")]
            [StringLength(500)]
            public string? AddressLine2 { get; set; }

            [Required]
            [Display(Name = "City")]
            [StringLength(100)]
            public string City { get; set; } = string.Empty;

            [Required]
            [Display(Name = "State/Province")]
            [StringLength(100)]
            public string State { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Postal Code")]
            [StringLength(20)]
            public string PostalCode { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Country")]
            [StringLength(100)]
            public string Country { get; set; } = "USA";

            [Phone]
            [Display(Name = "Phone Number")]
            [StringLength(20)]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Set as Primary Address")]
            public bool IsPrimary { get; set; }

            [Display(Name = "Set as Default Shipping Address")]
            public bool IsShippingDefault { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var profile = await _unitOfWork.UserProfiles.GetByUserIdAsync(userId);
            if (profile == null)
            {
                TempData["ErrorMessage"] = "Please complete your profile first.";
                return RedirectToPage("/Profile/Index");
            }

            UserProfileId = profile.Id;
            Addresses = await _unitOfWork.UserAddresses.GetByUserProfileIdAsync(profile.Id);

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!ModelState.IsValid) return await OnGetAsync();

            var userId = _userManager.GetUserId(User);
            var profile = await _unitOfWork.UserProfiles.GetByUserIdAsync(userId!);
            if (profile == null) return RedirectToPage("/Profile/Index");

            var address = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserProfileId = profile.Id,
                Label = Input.Label,
                AddressLine1 = Input.AddressLine1,
                AddressLine2 = Input.AddressLine2,
                City = Input.City,
                State = Input.State,
                PostalCode = Input.PostalCode,
                Country = Input.Country,
                PhoneNumber = Input.PhoneNumber,
                IsPrimary = Input.IsPrimary,
                IsShippingDefault = Input.IsShippingDefault
            };

            await _unitOfWork.UserAddresses.AddAsync(address);

            // If set as primary or shipping, update others
            if (Input.IsPrimary) await _unitOfWork.UserAddresses.SetPrimaryAsync(address.Id, profile.Id);
            if (Input.IsShippingDefault) await _unitOfWork.UserAddresses.SetShippingDefaultAsync(address.Id, profile.Id);

            TempData["SuccessMessage"] = "Address added successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!ModelState.IsValid || !Input.Id.HasValue) return await OnGetAsync();

            var address = await _unitOfWork.UserAddresses.GetByIdAsync(Input.Id.Value);
            if (address == null)
            {
                TempData["ErrorMessage"] = "Address not found.";
                return RedirectToPage();
            }

            address.Label = Input.Label;
            address.AddressLine1 = Input.AddressLine1;
            address.AddressLine2 = Input.AddressLine2;
            address.City = Input.City;
            address.State = Input.State;
            address.PostalCode = Input.PostalCode;
            address.Country = Input.Country;
            address.PhoneNumber = Input.PhoneNumber;
            address.IsPrimary = Input.IsPrimary;
            address.IsShippingDefault = Input.IsShippingDefault;

            await _unitOfWork.UserAddresses.UpdateAsync(address);

            if (Input.IsPrimary) await _unitOfWork.UserAddresses.SetPrimaryAsync(address.Id, address.UserProfileId);
            if (Input.IsShippingDefault) await _unitOfWork.UserAddresses.SetShippingDefaultAsync(address.Id, address.UserProfileId);

            TempData["SuccessMessage"] = "Address updated successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            await _unitOfWork.UserAddresses.DeleteAsync(id);
            TempData["SuccessMessage"] = "Address deleted successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSetPrimaryAsync(Guid id)
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _unitOfWork.UserProfiles.GetByUserIdAsync(userId!);
            if (profile == null) return RedirectToPage();

            await _unitOfWork.UserAddresses.SetPrimaryAsync(id, profile.Id);
            TempData["SuccessMessage"] = "Primary address updated!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSetShippingAsync(Guid id)
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _unitOfWork.UserProfiles.GetByUserIdAsync(userId!);
            if (profile == null) return RedirectToPage();

            await _unitOfWork.UserAddresses.SetShippingDefaultAsync(id, profile.Id);
            TempData["SuccessMessage"] = "Default shipping address updated!";
            return RedirectToPage();
        }
    }
}
