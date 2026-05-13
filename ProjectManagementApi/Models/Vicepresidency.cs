using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DatabasesLib;

namespace ProjectManagementApi.Models
{
    [Table("Vicepresidencies")]
    public class Vicepresidency : Entity
    {
        [Required]
        [Column("Name")]
        public string Name { get; set; }
    }
}
