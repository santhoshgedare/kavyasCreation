using Core.Entities;

namespace Core.Interfaces
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order);
        Task<IReadOnlyList<Order>> ListByUserAsync(string userId);
    }
}
