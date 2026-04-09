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
        public string? ProjectName { get; set; }
        public string? Sponsor { get; set; }
        public string? FunctionalLead { get; set; }
        public string? TechnicalLead { get; set; }
        public string? DocumentStatus { get; set; }
        public string? ProjectVision { get; set; }
        public string? GeneralObjective { get; set; }
        public string? SpecificObjectives { get; set; }
        public string? ExpectedValue { get; set; }
        public string? Scope { get; set; }
        public string? Exclusions { get; set; }
    }

    public class UpdateArchitectureSectionDto
    {
        public string? SolutionDescription { get; set; }
        public string? SolutionType { get; set; }
        public string? DeploymentModel { get; set; }
        public string? SoftwareStack { get; set; }
        public string? HardwareArchitecture { get; set; }
        public string? SecurityControl { get; set; }
        public int? ExpectedConcurrentUsers { get; set; }
        public string? SlaResponseTime { get; set; }
    }

    public class UpdateUxCasesSectionDto
    {
        public string? UseCases { get; set; }
        public string? RequiredDiagrams { get; set; }
        public string? ExperienceDesignMockups { get; set; }
        public string? TargetUsers { get; set; }
    }

    public class UpdateConstraintsSectionDto
    {
        public string? EstimatedBudget { get; set; }
        public DateTime? TargetDate { get; set; }
        public string? TechnicalConstraints { get; set; }
        public string? BusinessConstraints { get; set; }
        public string? RegulationsCompliance { get; set; }
    }

    public class UpdateAreasIntegrationsSectionDto
    {
        public string? InvolvedAreas { get; set; }
        public string? OrganizationalImpact { get; set; }
        public string? MasterDataMigration { get; set; }
    }

    public class UpdateRaciSectionDto
    {
        public string? ResponsibilitiesSummary { get; set; }
        public string? ChangeManagementAdoption { get; set; }
        public string? OperationSupport { get; set; }
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
        // Arquitectura
        public string? SolutionDescription { get; set; }
        public string? SolutionType { get; set; }
        public string? DeploymentModel { get; set; }
        public string? SoftwareStack { get; set; }
        public string? HardwareArchitecture { get; set; }
        public string? SecurityControl { get; set; }
        public int? ExpectedConcurrentUsers { get; set; }
        public string? SlaResponseTime { get; set; }
        // Casos y UX
        public string? UseCases { get; set; }
        public string? RequiredDiagrams { get; set; }
        public string? ExperienceDesignMockups { get; set; }
        public string? TargetUsers { get; set; }
        // Restricciones
        public string? EstimatedBudget { get; set; }
        public DateTime? TargetDate { get; set; }
        public string? TechnicalConstraints { get; set; }
        public string? BusinessConstraints { get; set; }
        public string? RegulationsCompliance { get; set; }
        // Áreas e Integraciones
        public string? InvolvedAreas { get; set; }
        public string? OrganizationalImpact { get; set; }
        public string? MasterDataMigration { get; set; }
        // RACI
        public string? ResponsibilitiesSummary { get; set; }
        public string? ChangeManagementAdoption { get; set; }
        public string? OperationSupport { get; set; }
        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public List<ProjectDocumentAttachmentDto> Attachments { get; set; }

        // Section Status (calculated at runtime)
        public string? GeneralSectionStatus { get; set; }
        public string? ArchitectureSectionStatus { get; set; }
        public string? UxCasesSectionStatus { get; set; }
        public string? ConstraintsSectionStatus { get; set; }
        public string? AreasIntegrationsSectionStatus { get; set; }
        public string? RaciSectionStatus { get; set; }
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

    public class ProjectDocumentListDto
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
        public string? UseCases { get; set; }
        public string? RequiredDiagrams { get; set; }
        public string? ExperienceDesignMockups { get; set; }
        public string? TargetUsers { get; set; }
        public string? EstimatedBudget { get; set; }
        public DateTime? TargetDate { get; set; }
        public string? TechnicalConstraints { get; set; }
        public string? BusinessConstraints { get; set; }
        public string? RegulationsCompliance { get; set; }
        public string? InvolvedAreas { get; set; }
        public string? OrganizationalImpact { get; set; }
        public string? MasterDataMigration { get; set; }
        public string? ResponsibilitiesSummary { get; set; }
        public string? ChangeManagementAdoption { get; set; }
        public string? OperationSupport { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }

        // Section Status (calculated at runtime)
        public string? GeneralSectionStatus { get; set; }
        public string? ArchitectureSectionStatus { get; set; }
        public string? UxCasesSectionStatus { get; set; }
        public string? ConstraintsSectionStatus { get; set; }
        public string? AreasIntegrationsSectionStatus { get; set; }
        public string? RaciSectionStatus { get; set; }
    }
}