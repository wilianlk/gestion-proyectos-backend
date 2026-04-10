namespace ProjectManagementApi.DTO
{
    public class CreateRequirementDto
    {
        public int ProjectDocumentId { get; set; }
        public List<RequirementItemDto> Requirements { get; set; }
    }

}