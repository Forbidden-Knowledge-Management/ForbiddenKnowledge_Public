using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForbiddenKnowledge.Data.DbModels
{

    [Table("reference_documents", Schema = "public")]
    public class ReferenceDocument
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("title")]
        [MaxLength(255)]
        public required string Title { get; set; }

        [Required]
        [Column("description")]
        [MaxLength(511)]
        public required string Description { get; set; }

        [Required]
        [Column("file_name")]
        [MaxLength(255)]
        public required string FileName { get; set; }

        [Required]
        [Column("uploaded_at")]
        public DateTime UploadedAt { get; set; }

        [NotMapped]
        public string FileSizeDisplay { get; set; } = "Unknown";


    }
}
