using Microsoft.EntityFrameworkCore;

using ForbiddenKnowledge.Data;
using ForbiddenKnowledge.Data.DbModels;


namespace ForbiddenKnowledge.Services
{
    public class BlogPostService : IBlogPostService
    {
        private readonly IConfiguration _configuration;
        private readonly ForbiddenKnowledgeContext _forbiddenKnowledgeContext;
        private readonly ILogger _logger;

        public BlogPostService(ForbiddenKnowledgeContext forbiddenKnowledgeContext, ILogger<BlogPostService> logger, IConfiguration configuration)
        {
            _forbiddenKnowledgeContext = forbiddenKnowledgeContext;
            _logger = logger;
            _configuration = configuration;
        }


        public async Task<List<BlogPost>> GetBlogPosts()
        {
            return (await _forbiddenKnowledgeContext.BlogPosts
                                                    .Include(p => p.Author)
                                                    .Include(p => p.Category)
                                                    .Include(p => p.BlogPostTags)
                                                        .ThenInclude(bpt => bpt.BlogPostTagLookup)
                                                    .ToListAsync()
                   ).OrderByDescending(p => p.PublishedDateTime).ToList();
        }


        public async Task<BlogPost?> GetBlogPostById(int blogPostId)
        {
            BlogPost? post = _forbiddenKnowledgeContext.BlogPosts
                                                       .Include(p => p.Author)
                                                       .Include(p => p.Category)
                                                       .Include(p => p.BlogPostTags)
                                                           .ThenInclude(bpt => bpt.BlogPostTagLookup)
                                                       .FirstOrDefault(p => p.Id == blogPostId);
            if (post != null && post != default) 
            {
                string filePath;
                //so our dev environment is windows, but our prod and test environments are linux
                if (_configuration.GetValue<string>("EnvironmentName") == "Development")
                {
                    filePath = _configuration.GetValue<string>("BlogPostFilePath") + "\\" + post.Filename + ".html";
                }
                else
                {
                    filePath = _configuration.GetValue<string>("BlogPostFilePath") + "/" + post.Filename + ".html";
                }
                post.PostContent = File.Exists(filePath) ? await File.ReadAllTextAsync(filePath) : null;
            }
            return post;
        }



    }
}
