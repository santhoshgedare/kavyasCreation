using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace kavyasCreation.Areas.Admin.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IReadOnlyList<Product> Products { get; private set; } = [];

        public async Task OnGetAsync()
        {
            Products = await _unitOfWork.Products.ListAsync();
        }
    }
}
