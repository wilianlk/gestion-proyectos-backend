namespace ProjectManagementApi.DTO
{
    public class RequirementDto
    {
        public int Id { get; set; }
        public int ProjectDocumentId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? Priority { get; set; }
        public string? AcceptanceCriteria { get; set; }
    }

}