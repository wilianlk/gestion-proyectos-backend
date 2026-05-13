namespace ProjectManagementApi.DTO
{
    public class DocumentKpisDto
    {
        public int TotalDocuments { get; set; }
        public int CompletedDocuments { get; set; }
        public int InProgressDocuments { get; set; }
        public int PendingDocuments { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal IntegrationsCoverageRate { get; set; }
    }
}
