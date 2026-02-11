using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace kavyasCreation.Areas.Admin.Pages.Inventory
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryService _inventoryService;
        private readonly AppDbContext _db;

        public DashboardModel(IUnitOfWork unitOfWork, IInventoryService inventoryService, AppDbContext db)
        {
            _unitOfWork = unitOfWork;
            _inventoryService = inventoryService;
            _db = db;
        }

        public IReadOnlyList<Product> LowStockProducts { get; private set; } = [];
        public int TotalProducts { get; private set; }
        public int OutOfStockProducts { get; private set; }
        public int LowStockCount { get; private set; }
        public int ActiveReservations { get; private set; }
        public decimal TotalInventoryValue { get; private set; }

        public async Task OnGetAsync()
        {
            var allProducts = await _unitOfWork.Products.ListAsync();
            LowStockProducts = await _inventoryService.GetLowStockProductsAsync();
            
            TotalProducts = allProducts.Count;
            OutOfStockProducts = allProducts.Count(p => p.Stock <= 0);
            LowStockCount = LowStockProducts.Count;
            TotalInventoryValue = allProducts.Sum(p => p.Price * p.Stock);
            
            ActiveReservations = await _db.StockReservations
                .CountAsync(r => !r.IsCommitted && !r.IsCancelled && r.ExpiresAt > DateTime.UtcNow);
        }
    }
}
