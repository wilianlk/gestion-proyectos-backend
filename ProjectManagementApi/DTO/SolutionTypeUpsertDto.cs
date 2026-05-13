using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApi.DTO
{
    public class SolutionTypeUpsertDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
    }
}
