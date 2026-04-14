namespace ProjectManagementApi.DTO
{
    public class CreateRequirementDto
    {
        public string ProjectCode { get; set; } = null!;
        public List<RequirementItemDto> Requirements { get; set; } = null!;
    }

}