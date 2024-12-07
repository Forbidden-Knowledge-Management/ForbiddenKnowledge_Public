using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForbiddenKnowledge.Data.DbModels
{
    [Table("blog_post_tag_lookup", Schema = "public")]
    public class BlogPostTagLookup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public required string Name { get; set; }
    }
}
