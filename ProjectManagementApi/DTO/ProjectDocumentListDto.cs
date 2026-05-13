namespace ProjectManagementApi.DTO
{
    public class ProjectDocumentListDto
    {
        public int Id { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string? Sponsor { get; set; }
        public string? TechnicalLead { get; set; }
        public string DocumentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool HasIncompleteDocumentAlert { get; set; }
        public string? IncompleteDocumentAlertMessage { get; set; }
    }
}
