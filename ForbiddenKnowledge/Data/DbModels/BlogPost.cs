using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForbiddenKnowledge.Data.DbModels
{
    [Table("blog_post", Schema = "public")]
    public class BlogPost
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("title")]
        public required string Title { get; set; }

        [Required]
        [Column("author_id")]
        public int AuthorId { get; set; }

        // Navigation property
        public Author Author { get; set; } = null!;

        [Required]
        [Column("published_date_time")]
        public DateTime PublishedDateTime { get; set; }

        [Required]
        [Column("category_id")]
        public int CategoryId { get; set; }

        // Navigation property
        public BlogPostCategoryLookup Category { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        [Column("filename")]
        public required string Filename { get; set; }

        /// <summary>
        /// The actual content of each post is stored as HTML files saved to disk.
        /// The posts are originally written in LaTeX and then converted to HTML.
        /// The contents of the HTML file are read from disk and put into this string when the blog post is displayed.
        /// Keep in mind this may include all sorts of media like pictures, mathematical equations, graphs made in LaTeX, possibly even music or videos.
        /// </summary>
        [NotMapped]
        public string? PostContent { get; set; }

        /// <summary>
        /// This is specifically the path to the post's thumbnail image.
        /// This is used as a thumbnail on the page that lists all the posts.
        /// Could possibly be used on the page for looking at a specific post by putting this image on top.
        /// These images are stored as static files of the web app.
        /// Keep in mind the post content can have a bunch of other images and media. Those images exist only in the HTML file of the post content.
        /// </summary>
        [NotMapped]
        public string ImagePath => $"img/blog/{Filename}.png";

        // Navigation property
        public List<BlogPostTag> BlogPostTags { get; set; } = new List<BlogPostTag>();

        public List<BlogPostComment> BlogPostComments { get; set; } = new List<BlogPostComment>();
    }
}
