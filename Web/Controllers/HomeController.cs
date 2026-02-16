using System.Diagnostics;
using Core.Interfaces;
using Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // Redirect to catalog as the new home page
            return RedirectToPage("/Catalog/Index", new { area = "Store" });
        }

        [AllowAnonymous]
        public async Task<IActionResult> About()
        {
            // Get featured/latest products for about page
            var featuredProducts = await _unitOfWork.Products.GetFeaturedAsync(8);
            return View(featuredProducts);
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

