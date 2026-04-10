namespace ProjectManagementApi.DTO
{
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

}