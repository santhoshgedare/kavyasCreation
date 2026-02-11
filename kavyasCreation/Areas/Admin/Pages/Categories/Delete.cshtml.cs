using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace kavyasCreation.Areas.Admin.Pages.Categories
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
            await _unitOfWork.Categories.DeleteAsync(Category.Id);
            return RedirectToPage("/Categories/Index", new { area = "Admin" });
        }
    }
}
