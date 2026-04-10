using DatabasesLib.Interfaces;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    public interface IProjectDocumentRaciActorRepository<T> : IRepository<T> where T : ProjectDocumentRaciActor
    {
        public Task<List<ProjectDocumentRaciActor>> GetByProjectDocumentIdAsync(int projectDocumentId);
        public Task DeleteByProjectCodeAsync(string projectCode);
        public void AddRange(List<ProjectDocumentRaciActor> entities);
        public Task SaveChangesAsync();
    }
}
