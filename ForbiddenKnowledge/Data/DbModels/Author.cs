using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForbiddenKnowledge.Data.DbModels
{
    [Table("author", Schema = "public")]
    public class Author
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public required string Name { get; set; }

        [Required]
        [MaxLength(1500)]
        [Column("biography")]
        public required string Biography { get; set; }

        [NotMapped]
        public string Email => $"{Name}@forbidden-knowledge.com";

        [NotMapped]
        public string ImagePath => $"img/authors/{Name}.png";


    }
}
