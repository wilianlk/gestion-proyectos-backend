using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApi.DTO
{
    public class TokenResponseDto
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}