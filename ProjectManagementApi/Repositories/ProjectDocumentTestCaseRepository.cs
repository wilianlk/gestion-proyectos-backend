using DatabasesLib;
using ProjectManagementApi.Context;
using ProjectManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjectManagementApi.Repositories
{
    public class ProjectDocumentTestCaseRepository : InformixBaseRepository<ProjectDocumentTestCase>, IProjectDocumentTestCaseRepository<ProjectDocumentTestCase>
    {
        private readonly ApplicationContext _context;

        public ProjectDocumentTestCaseRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<ProjectDocumentTestCase>> GetByProjectDocumentIdAsync(int projectDocumentId)
        {
            return await _context.ProjectDocumentTestCases
                .Where(x => x.ProjectDocumentId == projectDocumentId)
                .ToListAsync();
        }

        public async Task DeleteByProjectCodeAsync(string projectCode)
        {
            var entities = await _context.ProjectDocumentTestCases
                .Include(x => x.ProjectDocument)
                .Where(x => x.ProjectDocument != null && x.ProjectDocument.ProjectCode == projectCode)
                .ToListAsync();
            
            _context.ProjectDocumentTestCases.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        public void AddRange(List<ProjectDocumentTestCase> entities)
        {
            _context.ProjectDocumentTestCases.AddRange(entities);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
