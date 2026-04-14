using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DatabasesLib;

namespace ProjectManagementApi.Models
{
    public class Attachment : Entity
    {
        [Required]
        [Column("ProjectDocumentId")]
        public int ProjectDocumentId { get; set; }

        [ForeignKey("ProjectDocumentId")]
        public ProjectDocument ProjectDocument { get; set; }

        [Required]
        [Column("Section")]
        public string Section { get; set; }

        [Required]
        [Column("FileName")]
        public string FileName { get; set; }

        [Required]
        [Column("FilePath")]
        public string FilePath { get; set; }

        [Column("FileSize")]
        public long FileSize { get; set; }

        [Column("ContentType")]
        public string ContentType { get; set; }
    }
}