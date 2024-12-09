using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForbiddenKnowledge.Data.DbModels
{
    [Table("external_user", Schema = "public")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("pseudonym")]
        public required string Pseudonym { get; set; }

        [MaxLength(256)]
        [Column("email")]
        public string? Email { get; set; }

        [MaxLength(128)]
        [Column("password_hash")]
        public string? PasswordHash { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }


        // Required by Identity for case-insensitive email comparison
        [NotMapped]
        public string? NormalizedEmail => Email?.ToUpperInvariant();

    }
}
