using ForbiddenKnowledge.Data.DbModels;

namespace ForbiddenKnowledge.Services
{
    public interface IBlogPostService
    {
        Task<List<BlogPost>> GetBlogPosts();

        Task<BlogPost?> GetBlogPostById(int blogPostId);









    }
}
