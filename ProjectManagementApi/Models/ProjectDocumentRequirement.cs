using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DatabasesLib;

namespace ProjectManagementApi.Models
{
    public class ProjectDocumentRequirement : Entity
    {
        [Required]
        [Column("ProjectDocumentId")]
        public int ProjectDocumentId { get; set; }

        [ForeignKey("ProjectDocumentId")]
        public ProjectDocument ProjectDocument { get; set; }

        [Column("Code")]
        public string? Code { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        [Column("Type")]
        public string? Type { get; set; }

        [Column("Priority")]
        public string? Priority { get; set; }

        [Column("AcceptanceCriteria")]
        public string? AcceptanceCriteria { get; set; }
        [Column("Identification")]
        public string Identification { get; set; }

        [Column("Username")]
        public string Username { get; set; }

    }
}