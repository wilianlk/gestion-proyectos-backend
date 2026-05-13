using DatabasesLib;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.Context;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    public class SolutionTypeRepository : InformixBaseRepository<SolutionType>, ISolutionTypeRepository
    {
        private readonly ApplicationContext _context;

        public SolutionTypeRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public Task<List<SolutionType>> GetAllOrderedAsync()
        {
            return _context.SolutionTypes
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.SolutionTypes.AnyAsync(x => x.Name == name);
        }

        public async Task<bool> ExistsByNameExcludingIdAsync(int id, string name)
        {
            return await _context.SolutionTypes.AnyAsync(x => x.Id != id && x.Name == name);
        }

        public async Task<SolutionType> CreateAsync(string name)
        {
            var entity = new SolutionType
            {
                Name = name,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.SolutionTypes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<SolutionType?> UpdateAsync(int id, string name)
        {
            var entity = await _context.SolutionTypes.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return null;
            }

            entity.Name = name;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.SolutionTypes.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return false;
            }

            _context.SolutionTypes.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
