using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DatabasesLib;

namespace ProjectManagementApi.Models
{
    public class ProjectDocumentIntegration : Entity
    {
        [Required]
        [Column("ProjectDocumentId")]
        public int ProjectDocumentId { get; set; }

        [Column("System")]
        public string? System { get; set; }

        [Column("Description")]
        public string? Description { get; set; }
        
        [Column("Identification")]
        public string Identification { get; set; }

        [Column("Username")]
        public string Username { get; set; }

        public ProjectDocument? ProjectDocument { get; set; }
    }
}