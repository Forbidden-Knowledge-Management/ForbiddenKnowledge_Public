using ForbiddenKnowledge.Data.DbModels;

namespace ForbiddenKnowledge.Services
{
    public interface IBlogPostService
    {
        Task<List<BlogPost>> GetBlogPosts(int? categoryId = null, int? tagId = null, int? tagIdChapter = null, int? tagIdSection = null);

        Task<BlogPost?> GetBlogPostSimpleById(int blogPostId);

        Task<BlogPost?> GetBlogPostById(int blogPostId);

        Task<List<BlogPostCategoryLookup>> GetCategoriesAsync();

        Task<List<BlogPostTagLookup>> GetGenericTagsAsync();

        Task<List<BlogPostTagLookup>> GetFKChaptersAsync();

        Task<List<BlogPostTagLookup>> GetFKSectionsAsync(int? tagIdOfFKChapter = null);

        Task<int> GetBlogPostPageViews(int blogPostId);

        Task<(bool Succeeded, string? Error)> SubmitNewComment(BlogPostComment blogPostComment);

        Task<List<ReferenceDocument>> GetReferenceDocuments();









    }
}
