namespace ProjectManagementApi.DTO
{
    public class AttachmentDto
    {
        public int Id { get; set; }
        public int ProjectDocumentId { get; set; }
        public string Section { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string? ReferenceCode { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

}
