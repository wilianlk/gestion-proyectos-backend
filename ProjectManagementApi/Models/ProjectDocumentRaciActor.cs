using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DatabasesLib;

namespace ProjectManagementApi.Models
{
    public class ProjectDocumentRaciActor : Entity
    {
        [Required]
        [Column("ProjectDocumentId")]
        public int ProjectDocumentId { get; set; }

        [Column("Activity")]
        public string? Activity { get; set; }

        [Column("Type")]
        public string? Type { get; set; }

        [Column("Area")]
        public string? Area { get; set; }

        [Column("Role")]
        public string? Role { get; set; }
        
        [Column("Identification")]
        public string Identification { get; set; }

        [Column("Username")]
        public string Username { get; set; }

        public ProjectDocument? ProjectDocument { get; set; }
    }
}
