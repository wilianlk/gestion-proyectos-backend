using DatabasesLib;
using ProjectManagementApi.Context;
using ProjectManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjectManagementApi.Repositories
{
    public class ProjectDocumentIntegrationRepository : InformixBaseRepository<ProjectDocumentIntegration>, IProjectDocumentIntegrationRepository<ProjectDocumentIntegration>
    {
        private readonly ApplicationContext _context;

        public ProjectDocumentIntegrationRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<ProjectDocumentIntegration>> GetByProjectDocumentIdAsync(int projectDocumentId)
        {
            return await _context.ProjectDocumentIntegrations
                .Where(x => x.ProjectDocumentId == projectDocumentId)
                .ToListAsync();
        }

        public async Task DeleteByProjectCodeAsync(string projectCode)
        {
            var entities = await _context.ProjectDocumentIntegrations
            .Include(x => x.ProjectDocument)
                .Where(x => x.ProjectDocument != null && x.ProjectDocument.ProjectCode == projectCode)
                .ToListAsync();
            
            _context.ProjectDocumentIntegrations.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        public void AddRange(List<ProjectDocumentIntegration> entities)
        {
            _context.ProjectDocumentIntegrations.AddRange(entities);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}