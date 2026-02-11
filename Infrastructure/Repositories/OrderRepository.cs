using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _db;
        public OrderRepository(AppDbContext db) => _db = db;

        public async Task AddAsync(Order order)
        {
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<Order>> ListByUserAsync(string userId)
        {
            return await _db.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
    }
}
