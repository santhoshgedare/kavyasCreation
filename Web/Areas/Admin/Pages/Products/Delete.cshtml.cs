using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Admin.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product is null)
            {
                return RedirectToPage("/Products/Index", new { area = "Admin" });
            }

            Product = product;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _unitOfWork.Products.DeleteAsync(Product.Id);
            return RedirectToPage("/Products/Index", new { area = "Admin" });
        }
    }
}
