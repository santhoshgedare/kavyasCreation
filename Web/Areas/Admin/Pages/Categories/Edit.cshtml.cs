using Core.Entities;
using Core.Interfaces;
using Infra.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Web.Areas.Admin.Pages.Categories
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _db;

        public EditModel(IUnitOfWork unitOfWork, AppDbContext db)
        {
            _unitOfWork = unitOfWork;
            _db = db;
        }

        [BindProperty]
        public Category Category { get; set; } = new();

        [BindProperty]
        public List<string> VariantTypeNames { get; set; } = [];

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var category = await _unitOfWork.Categories.GetByIdWithVariantTypesAsync(id);
            if (category is null)
            {
                return RedirectToPage("/Categories/Index", new { area = "Admin" });
            }

            Category = category;
            VariantTypeNames = category.VariantTypes
                .OrderBy(v => v.SortOrder)
                .Select(v => v.Name)
                .ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Remove old variant types
            var existingTypes = await _db.CategoryVariantTypes
                .Where(v => v.CategoryId == Category.Id)
                .ToListAsync();
            _db.CategoryVariantTypes.RemoveRange(existingTypes);

            // Add new variant types
            var sortOrder = 0;
            foreach (var name in VariantTypeNames.Where(n => !string.IsNullOrWhiteSpace(n)))
            {
                _db.CategoryVariantTypes.Add(new CategoryVariantType
                {
                    Id = Guid.NewGuid(),
                    CategoryId = Category.Id,
                    Name = name.Trim(),
                    SortOrder = sortOrder++
                });
            }

            await _unitOfWork.Categories.UpdateAsync(Category);
            return RedirectToPage("/Categories/Index", new { area = "Admin" });
        }
    }
}
