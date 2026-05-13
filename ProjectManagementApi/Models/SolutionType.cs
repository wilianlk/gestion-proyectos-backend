using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DatabasesLib;

namespace ProjectManagementApi.Models
{
    [Table("SolutionTypes")]
    public class SolutionType : Entity
    {
        [Required]
        [Column("Name")]
        public string Name { get; set; }
    }
}
