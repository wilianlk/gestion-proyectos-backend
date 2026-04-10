using DatabasesLib.Interfaces;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    public interface IProjectDocumentRequirementRepository<T> : IRepository<T> where T : ProjectDocumentRequirement
    {
        public Task<List<ProjectDocumentRequirement>> GetByProjectDocumentIdAsync(int projectDocumentId);
        public Task DeleteByProjectCodeAsync(string projectCode);
        public void AddRange(List<ProjectDocumentRequirement> entities);
        public Task SaveChangesAsync();
    }
}