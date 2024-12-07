using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForbiddenKnowledge.Data.DbModels
{
    [Table("blog_post_tag", Schema = "public")]
    [PrimaryKey(nameof(PostId), nameof(TagId))]
    public class BlogPostTag
    {
        [Column("post_id"), ForeignKey("BlogPost")]
        public int PostId { get; set; }

        [Column("tag_id"), ForeignKey("BlogPostTagLookup")]
        public int TagId { get; set; }


        // Navigation properties
        public required BlogPostTagLookup BlogPostTagLookup { get; set; }
    }
}
