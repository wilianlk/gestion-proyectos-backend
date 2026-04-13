using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DatabasesLib;

namespace ProjectManagementApi.Models
{
    public class ProjectDocumentTestCase : Entity
    {
        [Required]
        [Column("ProjectDocumentId")]
        public int ProjectDocumentId { get; set; }

        [ForeignKey("ProjectDocumentId")]
        public ProjectDocument ProjectDocument { get; set; }

        [Column("TestStrategy")]
        public string? TestStrategy { get; set; }

        [Column("AcceptanceCriteria")]
        public string? AcceptanceCriteria { get; set; }

        [Column("DeployProductionCriteria")]
        public string? DeployProductionCriteria { get; set; }
        
        [Column("Identification")]
        public string Identification { get; set; }

        [Column("Username")]
        public string Username { get; set; }
    }
}
