namespace ProjectManagementApi.DTO
{
    // DTOs for Integrations
    public class CreateIntegrationDto
    {
        public string ProjectCode { get; set; } = null!;
        public List<IntegrationItemDto> Integrations { get; set; } = null!;
    }
}