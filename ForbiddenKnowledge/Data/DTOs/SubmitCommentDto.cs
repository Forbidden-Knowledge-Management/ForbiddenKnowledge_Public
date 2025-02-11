namespace ForbiddenKnowledge.Data.DTOs
{
    public record SubmitCommentDto
    {
        public int BlogPostId { get; set; }
        public string Pseudonym { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;


    }
}
