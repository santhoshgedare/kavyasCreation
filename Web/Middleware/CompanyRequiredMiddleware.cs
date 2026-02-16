using Core.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Web.Middleware
{
    /// <summary>
    /// Ensures that Vendor and Buyer users are associated with a valid company.
    /// Redirects to error page if company is not found.
    /// </summary>
    public class CompanyRequiredMiddleware
    {
        private readonly RequestDelegate _next;

        public CompanyRequiredMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            UserManager<IdentityUser> userManager,
            IVendorUserRepository vendorUserRepository,
            IBuyerUserRepository buyerUserRepository,
            IVendorRepository vendorRepository,
            IBuyerCompanyRepository buyerCompanyRepository)
        {
            // Skip if not authenticated
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            // Skip for Admin and Customer roles
            if (context.User.IsInRole("Admin") || context.User.IsInRole("Customer"))
            {
                await _next(context);
                return;
            }

            // Skip for Identity pages (login, register, etc.)
            if (context.Request.Path.StartsWithSegments("/Identity"))
            {
                await _next(context);
                return;
            }

            // Skip for error pages
            if (context.Request.Path.StartsWithSegments("/Error"))
            {
                await _next(context);
                return;
            }

            // Skip for static files
            if (context.Request.Path.StartsWithSegments("/lib") ||
                context.Request.Path.StartsWithSegments("/css") ||
                context.Request.Path.StartsWithSegments("/js") ||
                context.Request.Path.StartsWithSegments("/images"))
            {
                await _next(context);
                return;
            }

            var user = await userManager.GetUserAsync(context.User);
            if (user == null)
            {
                await _next(context);
                return;
            }

            // Check Vendor users
            if (context.User.IsInRole("VendorAdmin") || context.User.IsInRole("VendorUser"))
            {
                var vendorUser = await vendorUserRepository.GetByUserIdAsync(user.Id);
                if (vendorUser == null)
                {
                    context.Response.Redirect("/Error/NoCompany?type=vendor");
                    return;
                }

                var vendor = await vendorRepository.GetByIdAsync(vendorUser.VendorId);
                if (vendor == null || vendor.IsDeleted || !vendor.IsActive)
                {
                    context.Response.Redirect("/Error/CompanyInactive?type=vendor");
                    return;
                }
            }

            // Check Buyer users
            if (context.User.IsInRole("BuyerAdmin") || context.User.IsInRole("BuyerUser"))
            {
                var buyerUser = await buyerUserRepository.GetByUserIdAsync(user.Id);
                if (buyerUser == null)
                {
                    context.Response.Redirect("/Error/NoCompany?type=buyer");
                    return;
                }

                var buyerCompany = await buyerCompanyRepository.GetByIdAsync(buyerUser.BuyerCompanyId);
                if (buyerCompany == null || buyerCompany.IsDeleted || !buyerCompany.IsActive)
                {
                    context.Response.Redirect("/Error/CompanyInactive?type=buyer");
                    return;
                }
            }

            await _next(context);
        }
    }

    public static class CompanyRequiredMiddlewareExtensions
    {
        public static IApplicationBuilder UseCompanyRequired(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CompanyRequiredMiddleware>();
        }
    }
}
