using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Admin.Pages.Categories
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public Category Category { get; set; } = new();

        [BindProperty]
        public List<string> VariantTypeNames { get; set; } = [];

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Category.Id = Guid.NewGuid();

            var sortOrder = 0;
            foreach (var name in VariantTypeNames.Where(n => !string.IsNullOrWhiteSpace(n)))
            {
                Category.VariantTypes.Add(new CategoryVariantType
                {
                    Id = Guid.NewGuid(),
                    CategoryId = Category.Id,
                    Name = name.Trim(),
                    SortOrder = sortOrder++
                });
            }

            await _unitOfWork.Categories.AddAsync(Category);
            return RedirectToPage("/Categories/Index", new { area = "Admin" });
        }
    }
}
