namespace ProjectManagementApi.Models
{
    public class ProjectDocumentCompletionSnapshot
    {
        public int Id { get; set; }
        public string? ProjectName { get; set; }
        public string? Sponsor { get; set; }
        public string? FunctionalLead { get; set; }
        public string? TechnicalLead { get; set; }
        public string? DocumentStatus { get; set; }
        public string? ProjectVision { get; set; }
        public string? GeneralObjective { get; set; }
        public string? SpecificObjectives { get; set; }
        public string? Scope { get; set; }
        public string? Exclusions { get; set; }
        public string? SolutionDescription { get; set; }
        public string? SoftwareStack { get; set; }
        public string? SecurityControl { get; set; }
        public string? HardwareArchitecture { get; set; }
        public bool HasArchitectureAttachment { get; set; }
        public bool HasUseCasesAttachment { get; set; }
        public bool HasUxAttachment { get; set; }
        public string? EstimatedBudget { get; set; }
        public DateTime? TargetDate { get; set; }
        public string? TechnicalConstraints { get; set; }
        public string? BusinessConstraints { get; set; }
        public string? RegulationsCompliance { get; set; }
        public bool HasCompleteRaciActor { get; set; }
        public bool HasAnyRequirementData { get; set; }
        public bool HasCompleteRequirement { get; set; }
        public bool HasAnyIntegrationData { get; set; }
        public bool HasCompleteIntegration { get; set; }
        public bool HasCompleteRisk { get; set; }
        public bool HasCompleteTestCase { get; set; }
    }
}
