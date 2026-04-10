using DatabasesLib.Interfaces;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    public interface IProjectDocumentTestCaseRepository<T> : IRepository<T> where T : ProjectDocumentTestCase
    {
        public Task<List<ProjectDocumentTestCase>> GetByProjectDocumentIdAsync(int projectDocumentId);
        public Task DeleteByProjectCodeAsync(string projectCode);
        public void AddRange(List<ProjectDocumentTestCase> entities);
        public Task SaveChangesAsync();
    }
}
