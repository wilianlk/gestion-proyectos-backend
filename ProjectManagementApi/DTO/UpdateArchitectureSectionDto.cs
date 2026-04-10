namespace ProjectManagementApi.DTO
{
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

}