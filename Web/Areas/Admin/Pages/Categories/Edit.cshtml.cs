using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Admin.Pages.Categories
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public EditModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public Category Category { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category is null)
            {
                return RedirectToPage("/Categories/Index", new { area = "Admin" });
            }

            Category = category;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _unitOfWork.Categories.UpdateAsync(Category);
            return RedirectToPage("/Categories/Index", new { area = "Admin" });
        }
    }
}
