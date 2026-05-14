namespace ProjectManagementApi.DTO
{
    public class CreateRoleRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Application { get; set; }
        public string? Type { get; set; }
    }
}
