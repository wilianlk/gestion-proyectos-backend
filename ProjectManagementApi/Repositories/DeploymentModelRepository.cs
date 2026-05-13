using DatabasesLib;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.Context;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    public class DeploymentModelRepository : InformixBaseRepository<DeploymentModel>, IDeploymentModelRepository
    {
        private readonly ApplicationContext _context;

        public DeploymentModelRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public Task<List<DeploymentModel>> GetAllOrderedAsync()
        {
            return _context.DeploymentModels
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.DeploymentModels.AnyAsync(x => x.Name == name);
        }

        public async Task<bool> ExistsByNameExcludingIdAsync(int id, string name)
        {
            return await _context.DeploymentModels.AnyAsync(x => x.Id != id && x.Name == name);
        }

        public async Task<DeploymentModel> CreateAsync(string name)
        {
            var entity = new DeploymentModel
            {
                Name = name,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.DeploymentModels.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<DeploymentModel?> UpdateAsync(int id, string name)
        {
            var entity = await _context.DeploymentModels.FirstOrDefaultAsync(x => x.Id == id);
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
            var entity = await _context.DeploymentModels.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return false;
            }

            _context.DeploymentModels.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
