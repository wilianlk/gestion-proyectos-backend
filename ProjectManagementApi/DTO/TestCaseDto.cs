namespace ProjectManagementApi.DTO
{
    public class TestCaseDto
    {
        public int Id { get; set; }
        public int ProjectDocumentId { get; set; }
        public string? TestStrategy { get; set; }
        public string? AcceptanceCriteria { get; set; }
        public string? DeployProductionCriteria { get; set; }
    }

}
