using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DatabasesLib;

namespace ProjectManagementApi.Models
{
    public class Menu : Entity
    {
        [Required]
        [Column("Level")]
        public int Level { get; set; }

        [Required]
        [Column("IsNavigate")]
        public bool IsNavigate { get; set; }

        [Required]
        [Column("Name")]
        public string Name { get; set; }

        [Required]
        [Column("Url")]
        public string Url { get; set; }

        [Column("IconMenu")]
        public string IconMenu { get; set; }

        [Required]
        [Column("IsErrorDetails")]
        public bool IsErrorDetails { get; set; }

        [Column("FirstParentId")]
        public int? FirstParentId { get; set; }

        [Column("SecondParentId")]
        public int? SecondParentId { get; set; }

        [ForeignKey("FirstParentId")]
        public Menu? FirstParent { get; set; }

        [ForeignKey("SecondParentId")]
        public Menu? SecondParent { get; set; }

        [NotMapped]
        public bool IsParent { get; set; }
    }
}