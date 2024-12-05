using ForbiddenKnowledge.Data.Models;

namespace ForbiddenKnowledge.Services
{
    public interface IBlogPostService
    {
        Task<List<BlogPost>> GetBlogPosts();

        Task<BlogPost?> GetBlogPostById(int blogPostId);









    }
}
