using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Admin.Pages.Inventory
{
    [Authorize(Roles = "Admin")]
    public class StockMovementsModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryService _inventoryService;

        public StockMovementsModel(IUnitOfWork unitOfWork, IInventoryService inventoryService)
        {
            _unitOfWork = unitOfWork;
            _inventoryService = inventoryService;
        }

        public IReadOnlyList<StockMovement> Movements { get; private set; } = [];
        public Product? Product { get; private set; }

        public async Task<IActionResult> OnGetAsync(Guid? productId)
        {
            if (productId.HasValue)
            {
                Product = await _unitOfWork.Products.GetByIdAsync(productId.Value);
                if (Product is null)
                {
                    return RedirectToPage("/Inventory/Dashboard", new { area = "Admin" });
                }
                Movements = await _inventoryService.GetStockHistoryAsync(productId.Value, 100);
            }

            return Page();
        }
    }
}
