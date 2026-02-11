using Core.Entities;
using Core.Interfaces;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _db;
        public CategoryRepository(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<Category>> ListAsync() => await _db.Categories.AsNoTracking().ToListAsync();

        public async Task<Category?> GetByIdAsync(Guid id) => await _db.Categories.FindAsync(id);

        public async Task AddAsync(Category category)
        {
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _db.Categories.Update(category);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _db.Categories.FindAsync(id);
            if (entity != null)
            {
                _db.Categories.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }
    }
}
