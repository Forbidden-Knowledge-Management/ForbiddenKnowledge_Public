using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

using ForbiddenKnowledge.Data;
using ForbiddenKnowledge.Data.DbModels;

using Npgsql;


namespace ForbiddenKnowledge.Services
{
    public class BlogPostService : IBlogPostService
    {
        private readonly IConfiguration _configuration;
        private readonly ForbiddenKnowledgeContext _forbiddenKnowledgeContext;
        private readonly AuditDbContext _auditDbContext;
        private readonly ILogger _logger;
        private readonly IWebHostEnvironment _env;

        private static readonly Regex SqlInjectionPattern = new(@"(--|;|--|\b(SELECT|INSERT|UPDATE|DELETE|DROP|ALTER|CREATE|EXEC)\b)", RegexOptions.IgnoreCase);

        public BlogPostService(ForbiddenKnowledgeContext forbiddenKnowledgeContext, ILogger<BlogPostService> logger, IConfiguration configuration, AuditDbContext auditDbContext, IWebHostEnvironment env)
        {
            _forbiddenKnowledgeContext = forbiddenKnowledgeContext;
            _logger = logger;
            _configuration = configuration;
            _auditDbContext = auditDbContext;
            _env = env;
        }


        public async Task<List<BlogPost>> GetBlogPosts(int? categoryId = null, int? tagId = null, int? tagIdChapter = null, int? tagIdSection = null)
        {
            IQueryable<BlogPost> query = _forbiddenKnowledgeContext.BlogPosts
                                                  .Include(p => p.Author)
                                                  .Include(p => p.Category)
                                                  .Include(p => p.BlogPostTags)
                                                      .ThenInclude(bpt => bpt.BlogPostTagLookup);

            if (categoryId != 0 && categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (tagId != 0 && tagId.HasValue)
            {
                query = query.Where(p => p.BlogPostTags.Any(bpt => bpt.TagId == tagId.Value));
            }

            if (tagIdChapter != 0 && tagIdChapter.HasValue)
            {
                query = query.Where(p => p.BlogPostTags.Any(bpt => bpt.TagId == tagIdChapter.Value));
            }

            if (tagIdSection != 0 && tagIdSection.HasValue)
            {
                query = query.Where(p => p.BlogPostTags.Any(bpt => bpt.TagId == tagIdSection.Value));
            }

            return await query.OrderByDescending(p => p.PublishedDateTime).ToListAsync();
        }

        public async Task<BlogPost?> GetBlogPostSimpleById(int blogPostId)
        {
            BlogPost? post = _forbiddenKnowledgeContext.BlogPosts
                                           .Include(p => p.Author)
                                           .FirstOrDefault(p => p.Id == blogPostId);

            return post;
        }

        public async Task<BlogPost?> GetBlogPostById(int blogPostId)
        {
            BlogPost? post = _forbiddenKnowledgeContext.BlogPosts
                                                       .Include(p => p.Author)
                                                       .Include(p => p.Category)
                                                       .Include(p => p.BlogPostTags)
                                                           .ThenInclude(bpt => bpt.BlogPostTagLookup)
                                                       .Include(p => p.BlogPostComments)
                                                            .ThenInclude(bpc => bpc.User)
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

        public async Task<List<BlogPostCategoryLookup>> GetCategoriesAsync()
        {
            return await _forbiddenKnowledgeContext.BlogPostCategoryLookups.ToListAsync();
        }

        public async Task<List<BlogPostTagLookup>> GetGenericTagsAsync()
        {
            return await _forbiddenKnowledgeContext.BlogPostTagLookups.Where(t => t.TagType == "Generic").ToListAsync();
        }

        public async Task<List<BlogPostTagLookup>> GetFKChaptersAsync()
        {
            return await _forbiddenKnowledgeContext.BlogPostTagLookups.Where(t => t.TagType == "FK_Excerpt" && t.Name.StartsWith("Chapter")).ToListAsync();
        }

        public async Task<List<BlogPostTagLookup>> GetFKSectionsAsync(int? tagIdOfFKChapter = null)
        {
            IQueryable<BlogPostTagLookup> query = _forbiddenKnowledgeContext.BlogPostTagLookups.Where(t => t.TagType == "FK_Excerpt" && !t.Name.StartsWith("Chapter"));

            if (tagIdOfFKChapter != 0 && tagIdOfFKChapter.HasValue)
            {
                BlogPostTagLookup selectedFKChapterTag = _forbiddenKnowledgeContext.BlogPostTagLookups.FirstOrDefault(t => t.Id == tagIdOfFKChapter);

                Match match = Regex.Match(selectedFKChapterTag.Name, @"Chapter\s+(\d+):");
                if (match.Success)
                {
                    int chapter = int.Parse(match.Groups[1].Value);
                    query = query.Where(t => t.Chapter == chapter);
                }
            }

            return await query.ToListAsync();
        }

        public async Task<int> GetBlogPostPageViews(int blogPostId)
        {
            NpgsqlParameter blogPostIdParam = new NpgsqlParameter("blogPostIdParam", blogPostId);
            int result = await _auditDbContext.Database.SqlQueryRaw<int>("SELECT get_unique_blog_post_page_count(@blogPostIdParam)", blogPostIdParam).SingleAsync();
            return result;
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

            User user = await _forbiddenKnowledgeContext.Users.FirstOrDefaultAsync(u => u.Id == blogPostComment.UserId);
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
                _logger.LogError(ex, "Unexpected error submitting new comment.");
                return (false, $"An unexpected error occurred while submitting the comment: {ex.Message}");
            }
        }

        public async Task<List<ReferenceDocument>> GetReferenceDocuments()
        {
            List<ReferenceDocument> referenceDocuments = await _forbiddenKnowledgeContext.ReferenceDocuments.ToListAsync();

            foreach (ReferenceDocument doc in referenceDocuments)
            {
                try
                {
                    // Assuming files are in wwwroot/docs
                    string path = Path.Combine(_env.WebRootPath, "reference_docs", doc.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        long sizeBytes = new FileInfo(path).Length;
                        doc.FileSizeDisplay = FormatSize(sizeBytes);
                    }
                }
                catch
                {
                    doc.FileSizeDisplay = "Unknown";
                }
            }

            return referenceDocuments;
        }

        private bool ContainsSqlInjection(string input)
        {
            return SqlInjectionPattern.IsMatch(input);
        }

        private string FormatSize(long bytes)
        {
            if (bytes >= 1_048_576)
                return $"{bytes / 1_048_576.0:F2} MB";
            if (bytes >= 1024)
                return $"{bytes / 1024.0:F2} KB";
            return $"{bytes} bytes";
        }
    }
}
