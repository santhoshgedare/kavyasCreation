using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Admin.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class DeletedModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeletedModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IReadOnlyList<Product> Products { get; private set; } = [];

        public async Task OnGetAsync()
        {
            Products = await _unitOfWork.Products.ListDeletedAsync();
        }

        public async Task<IActionResult> OnPostRestoreAsync(Guid id)
        {
            await _unitOfWork.Products.RestoreAsync(id);
            return RedirectToPage();
        }
    }
}
