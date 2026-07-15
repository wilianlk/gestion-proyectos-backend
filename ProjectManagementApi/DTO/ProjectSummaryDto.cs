namespace ProjectManagementApi.DTO
{
    public class ProjectSummaryDto
    {
        public int Id { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string? Sponsor { get; set; }
        public string? FunctionalLead { get; set; }
        public string? TechnicalLead { get; set; }
        public string DocumentStatus { get; set; }
        public string Area { get; set; }
        public string Division { get; set; }
        public DateTime CreatedAt { get; set; }
        public int RequirementsCount { get; set; }
        public int RisksCount { get; set; }
        public int TestCasesCount { get; set; }
        public int IntegrationsCount { get; set; }
        public int RaciActorsCount { get; set; }
        public Dictionary<string, string> SectionStatuses { get; set; } = new();
    }
}
