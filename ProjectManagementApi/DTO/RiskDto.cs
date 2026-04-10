namespace ProjectManagementApi.DTO
{
    public class RiskDto
    {
        public int Id { get; set; }
        public int ProjectDocumentId { get; set; }
        public string? Risk { get; set; }
        public string? Impact { get; set; }
        public string? Probability { get; set; }
        public string? Mitigation { get; set; }
        public string? Owner { get; set; }
    }

}
