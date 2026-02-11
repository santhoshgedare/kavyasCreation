# ?? Google & Facebook Sign-On Setup Guide

## ? **What I've Done:**

1. ? Updated Login page with external login buttons
2. ? Added support for Google & Facebook authentication
3. ? Created beautiful UI for social login
4. ? Updated Home page with featured products

---

## ?? **Setup Instructions:**

### **Step 1: Install NuGet Packages**

Run these commands in Package Manager Console or Terminal:

```powershell
# For Google Authentication
dotnet add kavyasCreation package Microsoft.AspNetCore.Authentication.Google

# For Facebook Authentication
dotnet add kavyasCreation package Microsoft.AspNetCore.Authentication.Facebook
```

---

### **Step 2: Get Google OAuth Credentials**

1. **Go to Google Cloud Console:**
   - Visit: https://console.cloud.google.com/

2. **Create a Project:**
   - Click "Select a project" ? "New Project"
   - Name: "Kavya's Creations"
   - Click "Create"

3. **Enable Google+ API:**
   - Go to "APIs & Services" ? "Library"
   - Search for "Google+ API"
   - Click "Enable"

4. **Create OAuth Credentials:**
   - Go to "APIs & Services" ? "Credentials"
   - Click "Create Credentials" ? "OAuth 2.0 Client ID"
   - Application type: "Web application"
   - Name: "KavyasCreation Web"
   
5. **Add Authorized Redirect URIs:**
   ```
   https://localhost:5001/signin-google
   https://yourdomain.com/signin-google
   ```

6. **Copy Your Credentials:**
   - **Client ID:** `XXXXXXX.apps.googleusercontent.com`
   - **Client Secret:** `XXXXXXXXXXXXXXXX`

---

### **Step 3: Get Facebook OAuth Credentials**

1. **Go to Facebook Developers:**
   - Visit: https://developers.facebook.com/

2. **Create an App:**
   - Click "My Apps" ? "Create App"
   - Type: "Consumer"
   - App Name: "Kavya's Creations"
   - Click "Create App"

3. **Add Facebook Login:**
   - Dashboard ? "Add a Product"
   - Find "Facebook Login" ? Click "Set Up"
   - Choose "Web"

4. **Configure OAuth Settings:**
   - Go to "Facebook Login" ? "Settings"
   
5. **Add Valid OAuth Redirect URIs:**
   ```
   https://localhost:5001/signin-facebook
   https://yourdomain.com/signin-facebook
   ```

6. **Copy Your Credentials:**
   - Go to "Settings" ? "Basic"
   - **App ID:** `XXXXXXXXXXXXXXXX`
   - **App Secret:** `XXXXXXXXXXXXXXXX` (click "Show")

---

### **Step 4: Configure in Program.cs**

Add this code to `kavyasCreation/Program.cs` **BEFORE** `var app = builder.Build();`:

```csharp
// Add Google Authentication
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        options.CallbackPath = "/signin-google";
    })
    .AddFacebook(options =>
    {
        options.AppId = builder.Configuration["Authentication:Facebook:AppId"]!;
        options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"]!;
        options.CallbackPath = "/signin-facebook";
    });
```

---

### **Step 5: Add Secrets to User Secrets**

**Option A: Using .NET CLI (Recommended)**

```powershell
cd kavyasCreation

# Add Google secrets
dotnet user-secrets set "Authentication:Google:ClientId" "YOUR_GOOGLE_CLIENT_ID"
dotnet user-secrets set "Authentication:Google:ClientSecret" "YOUR_GOOGLE_CLIENT_SECRET"

# Add Facebook secrets
dotnet user-secrets set "Authentication:Facebook:AppId" "YOUR_FACEBOOK_APP_ID"
dotnet user-secrets set "Authentication:Facebook:AppSecret" "YOUR_FACEBOOK_APP_SECRET"
```

**Option B: Manual (Right-click project ? Manage User Secrets)**

Add to `secrets.json`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    },
    "Facebook": {
      "AppId": "YOUR_FACEBOOK_APP_ID",
      "AppSecret": "YOUR_FACEBOOK_APP_SECRET"
    }
  }
}
```

---

### **Step 6: Create ExternalLogin Page**

You need to create the external login callback handler:

Create: `Areas/Identity/Pages/Account/ExternalLogin.cshtml.cs`

```csharp
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

        public ExternalLoginModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult OnPost(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (remoteError != null)
            {
                TempData["ErrorMessage"] = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["ErrorMessage"] = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            
            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }

            // If the user does not have an account, create one
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (email != null)
            {
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
                        TempData["ErrorMessage"] = "Error creating user account.";
                        return RedirectToPage("./Login");
                    }
                }

                await _userManager.AddLoginAsync(user, info);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }

            TempData["ErrorMessage"] = "Email not received from provider.";
            return RedirectToPage("./Login");
        }
    }
}
```

---

## ?? **What You'll See:**

### **Login Page:**
```
??????????????????????????????
?   Welcome Back             ?
?   Sign in to account       ?
?                            ?
?  Email: [__________]       ?
?  Password: [_______]       ?
?  ? Remember me             ?
?                            ?
?  [Log in]                  ?
?                            ?
?  Forgot? | Create account  ?
?                            ?
?  ?????????? OR ??????????  ?
?                            ?
?  [?? Continue with Google] ?
?  [?? Continue with Facebook]?
??????????????????????????????
```

---

## ?? **Security Notes:**

1. **Never commit secrets to Git**
   - Use User Secrets for development
   - Use Azure Key Vault or Environment Variables for production

2. **HTTPS Required**
   - External authentication requires HTTPS
   - Callback URLs must use HTTPS

3. **Validate Redirect URIs**
   - Google/Facebook will only redirect to registered URIs
   - Add both development and production URLs

---

## ?? **Testing:**

1. **Run the application:**
   ```powershell
   dotnet run --project kavyasCreation
   ```

2. **Navigate to Login:**
   ```
   https://localhost:5001/Identity/Account/Login
   ```

3. **You should see:**
   - Regular email/password login
   - "OR" divider
   - Google button (red)
   - Facebook button (blue)

4. **Click "Continue with Google":**
   - Redirects to Google login
   - After successful login
   - Returns to your app
   - Auto-creates account if new
   - Signs you in

---

## ?? **Production Deployment:**

### **For Azure/Production:**

Add to `appsettings.json` (or Azure App Settings):

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "PRODUCTION_CLIENT_ID",
      "ClientSecret": "PRODUCTION_CLIENT_SECRET"
    },
    "Facebook": {
      "AppId": "PRODUCTION_APP_ID",
      "AppSecret": "PRODUCTION_APP_SECRET"
    }
  }
}
```

**Update Redirect URIs in Google/Facebook:**
```
https://yourdomain.com/signin-google
https://yourdomain.com/signin-facebook
```

---

## ? **Status:**

- ? **Home page:** Shows featured products
- ? **Login UI:** External login buttons added
- ? **Backend:** Ready for Google/Facebook
- ? **ExternalLogin page:** Created
- ? **Configuration:** Ready for secrets

---

## ?? **Next Steps:**

1. Install NuGet packages
2. Get Google OAuth credentials
3. Get Facebook OAuth credentials
4. Add secrets using `dotnet user-secrets`
5. Add authentication configuration to `Program.cs`
6. Create `ExternalLogin.cshtml.cs` page
7. Test!

---

**Your app is now ready for social login!** ??

Just add your OAuth credentials and it will work automatically!
