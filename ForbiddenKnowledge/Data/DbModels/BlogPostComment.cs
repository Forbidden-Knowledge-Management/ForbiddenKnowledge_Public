using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForbiddenKnowledge.Data.DbModels
{
    [Table("blog_post_comment", Schema = "public")]
    public class BlogPostComment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [ForeignKey("BlogPost")]
        [Column("blog_post_id")]
        public int BlogPostId { get; set; }
        public BlogPost BlogPost { get; set; } = null!;

        [Required]
        [ForeignKey("User")]
        [Column("pseudonym")]
        public required string Pseudonym { get; set; }
        public User User { get; set; } = null!;

        [Column("parent_comment_id")]
        public int ParentCommentId { get; set; }

        [Required]
        [Column("content")]
        public required string Content { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        //[Column("is_deleted")]
        //public bool IsDeleted { get; set; } = false;




    }
}
