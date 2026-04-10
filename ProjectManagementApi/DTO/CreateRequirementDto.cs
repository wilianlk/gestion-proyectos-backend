namespace ProjectManagementApi.DTO
{
    public class CreateRequirementDto
    {
        public string ProjectCode { get; set; }
        public List<RequirementItemDto> Requirements { get; set; }
    }

}