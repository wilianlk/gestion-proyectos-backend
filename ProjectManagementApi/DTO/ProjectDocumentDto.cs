namespace ProjectManagementApi.DTO
{
    public class CreateProjectDocumentDto
    {
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string Sponsor { get; set; }
        public string FunctionalLead { get; set; }
        public string TechnicalLead { get; set; }
        public string DocumentStatus { get; set; }
        public string ProjectVision { get; set; }
        public string GeneralObjective { get; set; }
        public string SpecificObjectives { get; set; }
        public string ExpectedValue { get; set; }
        public string Scope { get; set; }
        public string Exclusions { get; set; }
    }

    public class UpdateProjectGeneralSectionDto
    {
        public string ProjectName { get; set; }
        public string Sponsor { get; set; }
        public string FunctionalLead { get; set; }
        public string TechnicalLead { get; set; }
        public string DocumentStatus { get; set; }
        public string ProjectVision { get; set; }
        public string GeneralObjective { get; set; }
        public string SpecificObjectives { get; set; }
        public string ExpectedValue { get; set; }
        public string Scope { get; set; }
        public string Exclusions { get; set; }
    }

    public class UpdateArchitectureSectionDto
    {
        public string SolutionDescription { get; set; }
        public string SolutionType { get; set; }
        public string DeploymentModel { get; set; }
        public string SoftwareStack { get; set; }
        public string HardwareArchitecture { get; set; }
        public string SecurityControl { get; set; }
        public int? ExpectedConcurrentUsers { get; set; }
        public string SlaResponseTime { get; set; }
    }

    public class ProjectDocumentDto
    {
        public int Id { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string? Sponsor { get; set; }
        public string? FunctionalLead { get; set; }
        public string? TechnicalLead { get; set; }
        public string DocumentStatus { get; set; }
        public string? ProjectVision { get; set; }
        public string? GeneralObjective { get; set; }
        public string? SpecificObjectives { get; set; }
        public string? ExpectedValue { get; set; }
        public string? Scope { get; set; }
        public string? Exclusions { get; set; }
        public string? SolutionDescription { get; set; }
        public string? SolutionType { get; set; }
        public string? DeploymentModel { get; set; }
        public string? SoftwareStack { get; set; }
        public string? HardwareArchitecture { get; set; }
        public string? SecurityControl { get; set; }
        public int? ExpectedConcurrentUsers { get; set; }
        public string? SlaResponseTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ProjectDocumentAttachmentDto> Attachments { get; set; }
    }

    public class ProjectDocumentAttachmentDto
    {
        public int Id { get; set; }
        public int ProjectDocumentId { get; set; }
        public string Section { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}