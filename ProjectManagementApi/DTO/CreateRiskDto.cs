namespace ProjectManagementApi.DTO
{
    public class CreateRiskDto
    {
        public string ProjectCode { get; set; } = null!;
        public List<RiskItemDto> Risks { get; set; } = null!;
    }

}
