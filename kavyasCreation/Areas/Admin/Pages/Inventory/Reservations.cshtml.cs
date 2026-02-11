using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace kavyasCreation.Areas.Admin.Pages.Inventory
{
    [Authorize(Roles = "Admin")]
    public class ReservationsModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly IUnitOfWork _unitOfWork;

        public ReservationsModel(AppDbContext db, IUnitOfWork unitOfWork)
        {
            _db = db;
            _unitOfWork = unitOfWork;
        }

        public IReadOnlyList<StockReservation> ActiveReservations { get; private set; } = [];
        public IReadOnlyList<StockReservation> ExpiredReservations { get; private set; } = [];
        public IReadOnlyList<StockReservation> CommittedReservations { get; private set; } = [];

        public async Task OnGetAsync()
        {
            var allReservations = await _db.StockReservations
                .Include(r => r.Product)
                    .ThenInclude(p => p!.Category)
                .Include(r => r.Product)
                    .ThenInclude(p => p!.Images.OrderBy(i => i.SortOrder))
                .OrderByDescending(r => r.ReservedAt)
                .Take(100)
                .AsNoTracking()
                .ToListAsync();

            ActiveReservations = allReservations
                .Where(r => !r.IsCommitted && !r.IsCancelled && r.ExpiresAt > DateTime.UtcNow)
                .ToList();

            ExpiredReservations = allReservations
                .Where(r => !r.IsCommitted && r.IsCancelled)
                .Take(20)
                .ToList();

            CommittedReservations = allReservations
                .Where(r => r.IsCommitted)
                .Take(20)
                .ToList();
        }
    }
}
