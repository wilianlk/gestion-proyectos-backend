using DatabasesLib.Interfaces;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    public interface IProjectDocumentRiskRepository<T> : IRepository<T> where T : ProjectDocumentRisk
    {
        public Task<List<ProjectDocumentRisk>> GetByProjectDocumentIdAsync(int projectDocumentId);
        public Task DeleteByProjectCodeAsync(string projectCode);
        public void AddRange(List<ProjectDocumentRisk> entities);
        public Task SaveChangesAsync();
    }
}
