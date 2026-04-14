namespace ProjectManagementApi.DTO
{
    public class UpdateProjectGeneralSectionDto
    {
        public string ProjectName { get; set; } = null!;
        public string Sponsor { get; set; } = null!;
        public string FunctionalLead { get; set; } = null!;
        public string TechnicalLead { get; set; } = null!;
        public string DocumentStatus { get; set; } = null!;
        public string ProjectVision { get; set; } = null!;
        public string GeneralObjective { get; set; } = null!;
        public string SpecificObjectives { get; set; } = null!;
        public string ExpectedValue { get; set; } = null!;
        public string Scope { get; set; } = null!;
        public string Exclusions { get; set; } = null!;
    }

}