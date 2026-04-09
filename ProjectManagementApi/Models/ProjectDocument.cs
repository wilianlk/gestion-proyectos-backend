using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DatabasesLib;

namespace ProjectManagementApi.Models
{
    public class ProjectDocument : Entity
    {
        [Required]
        [Column("ProjectCode")]
        public string ProjectCode { get; set; }

        [Required]
        [Column("ProjectName")]
        public string ProjectName { get; set; }

        [Column("Sponsor")]
        public string? Sponsor { get; set; }

        [Column("FunctionalLead")]
        public string? FunctionalLead { get; set; }

        [Column("TechnicalLead")]
        public string? TechnicalLead { get; set; }

        [Required]
        [Column("DocumentStatus")]
        public string DocumentStatus { get; set; }

        [Column("ProjectVision")]
        public string? ProjectVision { get; set; }

        [Column("GeneralObjective")]
        public string? GeneralObjective { get; set; }

        [Column("SpecificObjectives")]
        public string? SpecificObjectives { get; set; }

        [Column("ExpectedValue")]
        public string? ExpectedValue { get; set; }

        [Column("Scope")]
        public string? Scope { get; set; }

        [Column("Exclusions")]
        public string? Exclusions { get; set; }

        [Column("SolutionDescription")]
        public string? SolutionDescription { get; set; }

        [Column("SolutionType")]
        public string? SolutionType { get; set; }

        [Column("DeploymentModel")]
        public string? DeploymentModel { get; set; }

        [Column("SoftwareStack")]
        public string? SoftwareStack { get; set; }

        [Column("HardwareArchitecture")]
        public string? HardwareArchitecture { get; set; }

        [Column("SecurityControl")]
        public string? SecurityControl { get; set; }

        [Column("ExpectedConcurrentUsers")]
        public int? ExpectedConcurrentUsers { get; set; }

        [Column("SlaResponseTime")]
        public string? SlaResponseTime { get; set; }

        // Sección Casos y UX
        [Column("UseCases")]
        public string? UseCases { get; set; }

        [Column("RequiredDiagrams")]
        public string? RequiredDiagrams { get; set; }

        [Column("ExperienceDesignMockups")]
        public string? ExperienceDesignMockups { get; set; }

        [Column("TargetUsers")]
        public string? TargetUsers { get; set; }

        // Sección Restricciones
        [Column("EstimatedBudget")]
        public string? EstimatedBudget { get; set; }

        [Column("TargetDate")]
        public DateTime? TargetDate { get; set; }

        [Column("TechnicalConstraints")]
        public string? TechnicalConstraints { get; set; }

        [Column("BusinessConstraints")]
        public string? BusinessConstraints { get; set; }

        [Column("RegulationsCompliance")]
        public string? RegulationsCompliance { get; set; }

        // Sección Áreas e Integraciones
        [Column("InvolvedAreas")]
        public string? InvolvedAreas { get; set; }

        [Column("OrganizationalImpact")]
        public string? OrganizationalImpact { get; set; }

        [Column("MasterDataMigration")]
        public string? MasterDataMigration { get; set; }

        // Sección RACI
        [Column("ResponsibilitiesSummary")]
        public string? ResponsibilitiesSummary { get; set; }

        [Column("ChangeManagementAdoption")]
        public string? ChangeManagementAdoption { get; set; }

        [Column("OperationSupport")]
        public string? OperationSupport { get; set; }

        [Column("Identification")]
        public string Identification { get; set; }

        [Column("Username")]
        public string Username { get; set; }

        public ICollection<ProjectDocumentAttachment> Attachments { get; set; }
    }
}