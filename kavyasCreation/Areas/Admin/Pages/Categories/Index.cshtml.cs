using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace kavyasCreation.Areas.Admin.Pages.Categories
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IReadOnlyList<Category> Categories { get; private set; } = [];

        public async Task OnGetAsync()
        {
            Categories = await _unitOfWork.Categories.ListAsync();
        }
    }
}
