using DatabasesLib;
using ProjectManagementApi.Context;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjectManagementApi.Repositories
{
    public class AttachmentRepository : InformixBaseRepository<Attachment>, IAttachmentRepository<Attachment>
    {
        private readonly ApplicationContext _context;

        public AttachmentRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Attachment> CreateAttachmentAsync(AttachmentDto dto)
        {
            var entity = new Attachment
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

            _context.Attachments.Add(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<List<Attachment>> GetAttachmentsByProjectAsync(int projectDocumentId)
        {
            return await _context.Attachments
                .Where(x => x.ProjectDocumentId == projectDocumentId)
                .ToListAsync();
        }

        public async Task DeleteAttachmentAsync(int id)
        {
            var entity = await _context.Attachments.FindAsync(id);
            if (entity != null)
            {
                _context.Attachments.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}