namespace ProjectManagementApi.DTO
{
    // DTOs for Integrations
    public class CreateIntegrationDto
    {
        public int ProjectDocumentId { get; set; }
        public List<IntegrationItemDto> Integrations { get; set; }
    }
}