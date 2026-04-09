using DatabasesLib.Interfaces;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    public interface IProjectDocumentAttachmentRepository<T> : IRepository<T> where T : ProjectDocumentAttachment
    {
        public Task<ProjectDocumentAttachment> CreateAttachmentAsync(ProjectDocumentAttachmentDto dto);
        public Task<List<ProjectDocumentAttachment>> GetAttachmentsByProjectAsync(int projectDocumentId);
        public Task DeleteAttachmentAsync(int id);
    }
}