using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DatabasesLib;

namespace ProjectManagementApi.Models
{
    public class ProjectDocumentRisk : Entity
    {
        [Required]
        [Column("ProjectDocumentId")]
        public int ProjectDocumentId { get; set; }

        [ForeignKey("ProjectDocumentId")]
        public ProjectDocument ProjectDocument { get; set; }

        [Column("Risk")]
        public string? Risk { get; set; }

        [Column("Impact")]
        public string? Impact { get; set; }

        [Column("Probability")]
        public string? Probability { get; set; }

        [Column("Mitigation")]
        public string? Mitigation { get; set; }

        [Column("Owner")]
        public string? Owner { get; set; }
        
        [Column("Identification")]
        public string Identification { get; set; }

        [Column("Username")]
        public string Username { get; set; }
    }
}
