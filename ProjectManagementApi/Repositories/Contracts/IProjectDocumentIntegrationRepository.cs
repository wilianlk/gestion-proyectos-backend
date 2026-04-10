using DatabasesLib.Interfaces;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    public interface IProjectDocumentIntegrationRepository<T> : IRepository<T> where T : ProjectDocumentIntegration
    {
        public Task<List<ProjectDocumentIntegration>> GetByProjectDocumentIdAsync(int projectDocumentId);
        public Task DeleteByProjectDocumentIdAsync(int projectDocumentId);
        public void AddRange(List<ProjectDocumentIntegration> entities);
        public Task SaveChangesAsync();
    }
}