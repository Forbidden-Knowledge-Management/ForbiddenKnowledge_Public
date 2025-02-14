using Microsoft.EntityFrameworkCore;

using ForbiddenKnowledge.Data;
using ForbiddenKnowledge.Data.DbModels;
using System.Text.RegularExpressions;


namespace ForbiddenKnowledge.Services
{
    public class BlogPostService : IBlogPostService
    {
        private readonly IConfiguration _configuration;
        private readonly ForbiddenKnowledgeContext _forbiddenKnowledgeContext;
        private readonly ILogger _logger;

        private static readonly Regex SqlInjectionPattern = new(@"(--|;|--|\b(SELECT|INSERT|UPDATE|DELETE|DROP|ALTER|CREATE|EXEC)\b)", RegexOptions.IgnoreCase);

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
                                                       .Include(p => p.BlogPostComments)
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


        public async Task<(bool Succeeded, string? Error)> SubmitNewComment(BlogPostComment blogPostComment)
        {
            if (blogPostComment == null)
            {
                return (false, "Comment cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(blogPostComment.Content))
            {
                return (false, "Comment content cannot be empty.");
            }

            if (blogPostComment.Content.Length <= 4)
            {
                return (false, "Comment must be at least 5 characters long.");
            }

            if (blogPostComment.Content.Length > 5000)
            {
                return (false, "Comment is too long. Maximum allowed length is 5000 characters.");
            }

            if (ContainsSqlInjection(blogPostComment.Content))
            {
                return (false, "Invalid input detected.");
            }

            bool blogPostExists = await _forbiddenKnowledgeContext.BlogPosts.AnyAsync(bp => bp.Id == blogPostComment.BlogPostId);

            if (!blogPostExists)
            {
                return (false, "The blog post you are commenting on does not exist.");
            }

            User user = await _forbiddenKnowledgeContext.Users.FirstOrDefaultAsync(u => u.OriginalPseudonym == blogPostComment.Pseudonym);
            if (user == null)
            {
                return (false, "Invalid Pseudonym. User does not exist.");
            }

            try
            {
                _forbiddenKnowledgeContext.BlogPostComments.Add(blogPostComment);
                await _forbiddenKnowledgeContext.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting new comment.");
                return (false, $"An error occurred while submitting the comment: {ex.Message}");
            }
        }

        private bool ContainsSqlInjection(string input)
        {
            return SqlInjectionPattern.IsMatch(input);
        }
    }
}
