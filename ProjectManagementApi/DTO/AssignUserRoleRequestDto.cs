namespace ProjectManagementApi.DTO
{
    public class AssignUserRoleRequestDto
    {
        public string Identification { get; set; } = string.Empty;
        public int RoleId { get; set; }
    }
}
