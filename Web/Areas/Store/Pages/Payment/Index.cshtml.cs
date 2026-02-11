using Core.Entities;
using Core.Interfaces;
using Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Areas.Store.Pages.Payment
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly CartService _cartService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryService _inventoryService;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(CartService cartService, IUnitOfWork unitOfWork, IInventoryService inventoryService, UserManager<IdentityUser> userManager)
        {
            _cartService = cartService;
            _unitOfWork = unitOfWork;
            _inventoryService = inventoryService;
            _userManager = userManager;
        }

        public IReadOnlyList<CartItem> Items { get; private set; } = [];
        public decimal Total { get; private set; }
        public bool PaymentComplete { get; private set; }
        public string? ErrorMessage { get; private set; }
        public List<Guid> ReservationIds { get; private set; } = [];
        public DateTime? ReservationExpiresAt { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Items = _cartService.GetItems();
            Total = _cartService.GetTotal();

            if (!Items.Any())
            {
                return Page();
            }

            // Check if we already have reservations for this session
            var existingReservationIds = HttpContext.Session.GetObject<List<Guid>>("ReservationIds");
            if (existingReservationIds != null && existingReservationIds.Any())
            {
                ReservationIds = existingReservationIds;
                var expiresAt = HttpContext.Session.GetObject<DateTime?>("ReservationExpiresAt");
                ReservationExpiresAt = expiresAt;
                
                // Check if reservations are still valid
                if (expiresAt.HasValue && expiresAt.Value > DateTime.UtcNow)
                {
                    return Page(); // Reservations still active
                }
            }

            // Create new reservations
            var userId = _userManager.GetUserId(User) ?? string.Empty;
            var stockIssues = new List<string>();
            var reservations = new List<StockReservation>();

            // Validate stock availability
            foreach (var item in Items)
            {
                var available = await _inventoryService.CheckAvailabilityAsync(item.ProductId, item.Quantity);
                if (!available)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    stockIssues.Add($"{item.Name}: Only {product?.AvailableStock ?? 0} available");
                }
            }

            if (stockIssues.Any())
            {
                ErrorMessage = "Insufficient stock: " + string.Join(", ", stockIssues);
                return Page();
            }

            // Reserve stock for all items (15 minutes)
            foreach (var item in Items)
            {
                var reservation = await _inventoryService.ReserveStockAsync(item.ProductId, userId, item.Quantity, 15);
                if (reservation != null)
                {
                    reservations.Add(reservation);
                }
            }

            if (reservations.Any())
            {
                ReservationIds = reservations.Select(r => r.Id).ToList();
                ReservationExpiresAt = reservations.First().ExpiresAt;
                
                // Store in session
                HttpContext.Session.SetObject("ReservationIds", ReservationIds);
                HttpContext.Session.SetObject("ReservationExpiresAt", ReservationExpiresAt);
                
                TempData["InfoMessage"] = $"Stock reserved for {reservations.Sum(r => r.Quantity)} items. Complete payment within 15 minutes.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Items = _cartService.GetItems();
            Total = _cartService.GetTotal();

            if (!Items.Any())
            {
                return RedirectToPage();
            }

            // Get existing reservations from session
            var reservationIds = HttpContext.Session.GetObject<List<Guid>>("ReservationIds");
            if (reservationIds == null || !reservationIds.Any())
            {
                ErrorMessage = "No active reservations found. Please refresh and try again.";
                return Page();
            }

            var userId = _userManager.GetUserId(User) ?? string.Empty;
            var orderId = Guid.NewGuid();

            // Create order
            var order = new Order
            {
                Id = orderId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Total = Total,
                Items = Items.Select(i => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = i.ProductId,
                    Name = i.Name,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            };

            await _unitOfWork.Orders.AddAsync(order);

            // Commit all reservations (reduces actual stock)
            foreach (var reservationId in reservationIds)
            {
                await _inventoryService.CommitReservationAsync(reservationId, orderId);
            }

            // Clear session
            HttpContext.Session.Remove("ReservationIds");
            HttpContext.Session.Remove("ReservationExpiresAt");

            PaymentComplete = true;
            _cartService.Clear();
            TempData["SuccessMessage"] = "Payment successful! Your order has been placed.";
            return Page();
        }
    }
}
