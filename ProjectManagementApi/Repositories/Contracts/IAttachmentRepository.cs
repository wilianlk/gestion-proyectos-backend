using DatabasesLib.Interfaces;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    public interface IAttachmentRepository<T> : IRepository<T> where T : Attachment
    {
        Task<Attachment> CreateAttachmentAsync(AttachmentDto dto);
        Task<List<Attachment>> GetAttachmentsByProjectAsync(int projectDocumentId);
        Task DeleteAttachmentAsync(int id);
    }
}