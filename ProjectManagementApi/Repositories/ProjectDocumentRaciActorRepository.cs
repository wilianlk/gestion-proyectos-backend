using DatabasesLib;
using ProjectManagementApi.Context;
using ProjectManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjectManagementApi.Repositories
{
    public class ProjectDocumentRaciActorRepository : InformixBaseRepository<ProjectDocumentRaciActor>, IProjectDocumentRaciActorRepository<ProjectDocumentRaciActor>
    {
        private readonly ApplicationContext _context;

        public ProjectDocumentRaciActorRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<ProjectDocumentRaciActor>> GetByProjectDocumentIdAsync(int projectDocumentId)
        {
            return await _context.ProjectDocumentRaciActors
                .Where(x => x.ProjectDocumentId == projectDocumentId)
                .ToListAsync();
        }

        public async Task DeleteByProjectCodeAsync(string projectCode)
        {
            var entities = await _context.ProjectDocumentRaciActors
            .Include(x => x.ProjectDocument)
                .Where(x => x.ProjectDocument != null && x.ProjectDocument.ProjectCode == projectCode)
                .ToListAsync();
            
            _context.ProjectDocumentRaciActors.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        public void AddRange(List<ProjectDocumentRaciActor> entities)
        {
            _context.ProjectDocumentRaciActors.AddRange(entities);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
