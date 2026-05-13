using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DatabasesLib;

namespace ProjectManagementApi.Models
{
    [Table("DeploymentModels")]
    public class DeploymentModel : Entity
    {
        [Required]
        [Column("Name")]
        public string Name { get; set; }
    }
}
