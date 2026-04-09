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

        [Column("Identification")]
        public string Identification { get; set; }

        [Column("Username")]
        public string Username { get; set; }

        public ICollection<ProjectDocumentAttachment> Attachments { get; set; }
    }
}