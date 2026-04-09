using DatabasesLib.Interfaces;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    public interface IProjectDocumentRepository<T> : IRepository<T> where T : ProjectDocument
    {
        public Task<ProjectDocument> CreateAsync(CreateProjectDocumentDto dto);
        public Task<ProjectDocument?> GetByProjectCodeAsync(string projectCode);
        public Task<ProjectDocument?> GetByProjectCodeWithAttachmentsAsync(string projectCode);
        public Task<bool> ExistsByProjectCodeAsync(string projectCode);
        public Task UpdateGeneralSectionAsync(string projectCode, UpdateProjectGeneralSectionDto dto);
        public Task UpdateArchitectureSectionAsync(string projectCode, UpdateArchitectureSectionDto dto);
        public Task UpdateUxCasesSectionAsync(string projectCode, UpdateUxCasesSectionDto dto);
        public Task UpdateConstraintsSectionAsync(string projectCode, UpdateConstraintsSectionDto dto);
        public Task UpdateAreasIntegrationsSectionAsync(string projectCode, UpdateAreasIntegrationsSectionDto dto);
        public Task UpdateRaciSectionAsync(string projectCode, UpdateRaciSectionDto dto);
    }
}