

namespace ProjectManagementApi.DTO
{
    public class UserDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string Username { get; set; } = null!;
        public string? Email { get; set; }
        public string? Identification { get; set; }
        public string RoleName { get; set; } = null!;
        public int RoleId { get; set; }
    }
}
