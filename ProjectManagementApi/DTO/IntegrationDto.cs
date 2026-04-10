namespace ProjectManagementApi.DTO
{
    public class IntegrationDto
    {
        public int Id { get; set; }
        public int ProjectDocumentId { get; set; }
        public string? System { get; set; }
        public string? Description { get; set; }
    }

}