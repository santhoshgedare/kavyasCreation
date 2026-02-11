using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace kavyasCreation.Areas.Admin.Pages.Inventory
{
    [Authorize(Roles = "Admin")]
    public class AdjustStockModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryService _inventoryService;

        public AdjustStockModel(IUnitOfWork unitOfWork, IInventoryService inventoryService)
        {
            _unitOfWork = unitOfWork;
            _inventoryService = inventoryService;
        }

        public Product Product { get; private set; } = new();
        
        [BindProperty]
        public int Quantity { get; set; }
        
        [BindProperty]
        public string MovementType { get; set; } = "Adjustment";
        
        [BindProperty]
        public string? Notes { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product is null)
            {
                return RedirectToPage("/Inventory/Dashboard", new { area = "Admin" });
            }

            Product = product;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            if (!ModelState.IsValid)
            {
                Product = (await _unitOfWork.Products.GetByIdAsync(id))!;
                return Page();
            }

            var userName = User.Identity?.Name ?? "Admin";
            var success = await _inventoryService.AdjustStockAsync(id, Quantity, MovementType, userName, null, Notes);

            if (success)
            {
                TempData["SuccessMessage"] = $"Stock adjusted successfully. {(Quantity > 0 ? "Added" : "Removed")} {Math.Abs(Quantity)} units.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to adjust stock. Please try again.";
            }

            return RedirectToPage("/Inventory/Dashboard", new { area = "Admin" });
        }
    }
}
