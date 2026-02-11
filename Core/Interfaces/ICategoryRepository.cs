using Core.Entities;

namespace Core.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IReadOnlyList<Category>> ListAsync();
        Task<Category?> GetByIdAsync(Guid id);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(Guid id);
    }
}
