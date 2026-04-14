namespace ProjectManagementApi.DTO
{
    public class UpdateArchitectureSectionDto
    {
        public string SolutionDescription { get; set; } = null!;
        public string SolutionType { get; set; } = null!;
        public string DeploymentModel { get; set; } = null!;
        public string SoftwareStack { get; set; } = null!;
        public string HardwareArchitecture { get; set; } = null!;
        public string SecurityControl { get; set; } = null!;
        public int ExpectedConcurrentUsers { get; set; }
        public string SlaResponseTime { get; set; } = null!;
    }

}