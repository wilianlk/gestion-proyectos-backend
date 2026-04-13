
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DatabasesLib;

namespace ProjectManagementApi.Models
{
    public class Role : Entity
    {
        [Required]
        [Column("Name")]
        public string Name { get; set; }

        [Column("Description")]
        public string? Description { get; set; }
    }
}