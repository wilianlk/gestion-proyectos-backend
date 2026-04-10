using DatabasesLib;
using ProjectManagementApi.Context;
using ProjectManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjectManagementApi.Repositories
{
    public class ProjectDocumentRequirementRepository : InformixBaseRepository<ProjectDocumentRequirement>, IProjectDocumentRequirementRepository<ProjectDocumentRequirement>
    {
        private readonly ApplicationContext _context;

        public ProjectDocumentRequirementRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<ProjectDocumentRequirement>> GetByProjectDocumentIdAsync(int projectDocumentId)
        {
            return await _context.ProjectDocumentRequirements
                .Where(x => x.ProjectDocumentId == projectDocumentId)
                .ToListAsync();
        }

        public async Task DeleteByProjectDocumentIdAsync(int projectDocumentId)
        {
            var entities = await _context.ProjectDocumentRequirements
                .Where(x => x.ProjectDocumentId == projectDocumentId)
                .ToListAsync();
            
            _context.ProjectDocumentRequirements.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        public void AddRange(List<ProjectDocumentRequirement> entities)
        {
            _context.ProjectDocumentRequirements.AddRange(entities);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}