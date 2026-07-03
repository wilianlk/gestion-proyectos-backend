using DatabasesLib.Interfaces;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    public interface IProjectDocumentRepository<T> : IRepository<T> where T : ProjectDocument
    {
        public Task<ProjectDocument> CreateAsync(CreateProjectDocumentDto dto, User user);
        public Task<ProjectDocument?> GetByProjectCodeAsync(string projectCode);
        public Task<ProjectDocument?> GetByProjectCodeDetailedAsync(string projectCode);
        public Task<bool> ExistsByProjectCodeAsync(string projectCode);
        public Task UpdateGeneralSectionAsync(string projectCode, UpdateProjectGeneralSectionDto dto, User user);
        public Task UpdateArchitectureSectionAsync(string projectCode, UpdateArchitectureSectionDto dto, User user);
        public Task UpdateUxCasesSectionAsync(string projectCode, UpdateUxCasesSectionDto dto, User user);
        public Task UpdateConstraintsSectionAsync(string projectCode, UpdateConstraintsSectionDto dto, User user);
        public Task UpdateAreasIntegrationsSectionAsync(string projectCode, UpdateAreasIntegrationsSectionDto dto, User user);
        public Task UpdateRaciSectionAsync(string projectCode, UpdateRaciSectionDto dto, User user);
        public Task<List<ProjectDocument>> GetAllOrderedAsync();
        public Task<Dictionary<int, ProjectDocument>> GetDetailedByIdsAsync(IEnumerable<int> ids);
    }
}