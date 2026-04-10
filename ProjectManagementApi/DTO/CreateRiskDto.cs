namespace ProjectManagementApi.DTO
{
    public class CreateRiskDto
    {
        public string ProjectCode { get; set; }
        public List<RiskItemDto> Risks { get; set; }
    }

}
