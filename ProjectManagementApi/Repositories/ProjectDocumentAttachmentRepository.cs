using DatabasesLib;
using ProjectManagementApi.Context;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjectManagementApi.Repositories
{
    public class ProjectDocumentAttachmentRepository : InformixBaseRepository<ProjectDocumentAttachment>, IProjectDocumentAttachmentRepository<ProjectDocumentAttachment>
    {
        private readonly ApplicationContext _context;

        public ProjectDocumentAttachmentRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        /**
        * Description: Create a new attachment
        * Input Parameters: 
        *      * dto (ProjectDocumentAttachmentDto): Attachment data
        * Output Parameters: Created attachment
        */
        public async Task<ProjectDocumentAttachment> CreateAttachmentAsync(ProjectDocumentAttachmentDto dto)
        {
            var entity = new ProjectDocumentAttachment
            {
                ProjectDocumentId = dto.ProjectDocumentId,
                Section = dto.Section,
                FileName = dto.FileName,
                FilePath = dto.FilePath,
                FileSize = dto.FileSize,
                ContentType = dto.ContentType,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.ProjectDocumentAttachments.Add(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        /**
        * Description: Get all attachments by project document id
        * Input Parameters: 
        *      * projectDocumentId (int): Project document id
        * Output Parameters: List of attachments
        */
        public async Task<List<ProjectDocumentAttachment>> GetAttachmentsByProjectAsync(int projectDocumentId)
        {
            return await _context.ProjectDocumentAttachments
                .Where(x => x.ProjectDocumentId == projectDocumentId)
                .ToListAsync();
        }

        /**
        * Description: Delete attachment by id
        * Input Parameters: 
        *      * id (int): Attachment id
        * Output Parameters: None
        */
        public async Task DeleteAttachmentAsync(int id)
        {
            var entity = await _context.ProjectDocumentAttachments.FindAsync(id);
            if (entity != null)
            {
                _context.ProjectDocumentAttachments.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}