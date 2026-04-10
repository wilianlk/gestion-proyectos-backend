using DatabasesLib;
using ProjectManagementApi.Context;
using ProjectManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjectManagementApi.Repositories
{
    public class ProjectDocumentRiskRepository : InformixBaseRepository<ProjectDocumentRisk>, IProjectDocumentRiskRepository<ProjectDocumentRisk>
    {
        private readonly ApplicationContext _context;

        public ProjectDocumentRiskRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<ProjectDocumentRisk>> GetByProjectDocumentIdAsync(int projectDocumentId)
        {
            return await _context.ProjectDocumentRisks
                .Where(x => x.ProjectDocumentId == projectDocumentId)
                .ToListAsync();
        }

        public async Task DeleteByProjectCodeAsync(string projectCode)
        {
            var entities = await _context.ProjectDocumentRisks
                .Include(x => x.ProjectDocument)
                .Where(x => x.ProjectDocument != null && x.ProjectDocument.ProjectCode == projectCode)
                .ToListAsync();
            
            _context.ProjectDocumentRisks.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        public void AddRange(List<ProjectDocumentRisk> entities)
        {
            _context.ProjectDocumentRisks.AddRange(entities);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
