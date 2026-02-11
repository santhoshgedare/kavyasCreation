using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace kavyasCreation.Areas.Identity.Pages.Account
{
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<IdentityUser> signInManager, 
            UserManager<IdentityUser> userManager,
            ILogger<ExternalLoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult OnPost(string provider, string? returnUrl = null)
        {
            _logger.LogInformation("External login initiated for provider: {Provider}", provider);
            
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (remoteError != null)
            {
                _logger.LogError("Error from external provider: {Error}", remoteError);
                TempData["ErrorMessage"] = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError("Error loading external login information");
                TempData["ErrorMessage"] = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            _logger.LogInformation("External login info received from {Provider}", info.LoginProvider);

            // Sign in the user with this external login provider if the user already has a login
            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, 
                info.ProviderKey, 
                isPersistent: false, 
                bypassTwoFactor: true);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with {Provider} provider", info.LoginProvider);
                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out");
                TempData["ErrorMessage"] = "User account locked out.";
                return RedirectToPage("./Login");
            }

            // If the user does not have an account, create one
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogError("Email claim not found from {Provider}", info.LoginProvider);
                TempData["ErrorMessage"] = "Email not received from provider.";
                return RedirectToPage("./Login");
            }

            _logger.LogInformation("Creating new account for email: {Email}", email);

            var user = await _userManager.FindByEmailAsync(email);
            
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true // Auto-confirm since it's from external provider
                };
                
                var createResult = await _userManager.CreateAsync(user);
                
                if (!createResult.Succeeded)
                {
                    _logger.LogError("Error creating user account: {Errors}", 
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    TempData["ErrorMessage"] = $"Error creating user account: {string.Join(", ", createResult.Errors.Select(e => e.Description))}";
                    return RedirectToPage("./Login");
                }
                
                _logger.LogInformation("User account created for {Email}", email);
            }

            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            
            if (!addLoginResult.Succeeded)
            {
                _logger.LogError("Error adding external login: {Errors}", 
                    string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
                TempData["ErrorMessage"] = "Error linking external login.";
                return RedirectToPage("./Login");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("User signed in successfully with {Provider}", info.LoginProvider);
            
            TempData["SuccessMessage"] = $"Successfully signed in with {info.LoginProvider}!";
            
            return LocalRedirect(returnUrl);
        }
    }
}

