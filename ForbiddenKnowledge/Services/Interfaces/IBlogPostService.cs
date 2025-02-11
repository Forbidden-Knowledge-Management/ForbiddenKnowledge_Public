using ForbiddenKnowledge.Data.DbModels;

namespace ForbiddenKnowledge.Services
{
    public interface IBlogPostService
    {
        Task<List<BlogPost>> GetBlogPosts();

        Task<BlogPost?> GetBlogPostById(int blogPostId);

        Task<(bool Succeeded, string? Error)> SubmitNewComment(BlogPostComment blogPostComment);









    }
}
