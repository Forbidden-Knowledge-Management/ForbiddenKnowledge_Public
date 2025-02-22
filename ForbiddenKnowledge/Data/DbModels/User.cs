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

        //Pseudonym are case-insensitive, HOWEVER, we need to ensure we store the the original casing for front-end display purposes.
        [Required]
        [MaxLength(100)]
        [Column("original_pseudonym")]
        public required string OriginalPseudonym { get; set; }

        //Pseudonym (UserNames) are case-insensitive.
        //I am not a fan of this personally, but this is needed for security and practicality reasons.
        //it makes it easier for users to login, and prevents malicious users from creating accounts with the same name but different casing (so called "username squatting")
        [Required]
        [MaxLength(100)]
        [Column("uppercased_pseudonym")]
        public required string UppercasedPseudonym { get; set; }

        [MaxLength(256)]
        [Column("email")]
        public string? Email { get; set; }

        [MaxLength(128)]
        [Column("password_hash")]
        public string? PasswordHash { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("security_stamp")]
        public string? SecurityStamp { get; set; }

        [Required]
        [Column("access_failed_count")]
        public int AccessFailedCount { get; set; }

        [Column("lockout_end")]
        public DateTime? LockoutEnd { get; set; }

        [Column("lockout_enabled")]
        public bool LockoutEnabled { get; set; }

        // Required by Identity for case-insensitive email comparison
        [NotMapped]
        public string? NormalizedEmail => Email?.ToUpperInvariant();

    }
}
