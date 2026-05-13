namespace ProjectManagementApi.DTO
{
    public class UpdateProjectGeneralSectionDto
    {
        public string ProjectName { get; set; } = null!;
        public string Sponsor { get; set; } = null!;
        public string FunctionalLead { get; set; } = null!;
        public string TechnicalLead { get; set; } = null!;
        public string DocumentStatus { get; set; } = null!;
        public string Area { get; set; } = null!;
        public string Division { get; set; } = null!;
        public string? ProjectVision { get; set; }
        public string? GeneralObjective { get; set; }
        public string? SpecificObjectives { get; set; }
        public string? Scope { get; set; }
        public string? Exclusions { get; set; }
    }

}
