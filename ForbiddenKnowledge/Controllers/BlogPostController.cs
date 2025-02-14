using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using ForbiddenKnowledge.Data.DbModels;
using ForbiddenKnowledge.Services;
using ForbiddenKnowledge.Data.DTOs;



namespace ForbiddenKnowledge.Controllers
{
    [Route("api/blogposts")]
    [ApiController]
    public class BlogPostController : Controller
    {

        private readonly IBlogPostService _blogPostService;

        public BlogPostController(IBlogPostService blogPostService)
        {
            _blogPostService = blogPostService;
        }

        [HttpGet]
        public async Task<ActionResult<List<BlogPost>>> GetBlogPosts()
        {
            return await _blogPostService.GetBlogPosts();
        }

        [HttpGet("{blogPostId}")]
        public async Task<ActionResult<BlogPost>> GetBlogPost(int blogPostId)
        {
            BlogPost? post = await _blogPostService.GetBlogPostById(blogPostId);

            if (post == null)
            {
                return NotFound();
            }

            return post;
        }

        [Authorize]
        [HttpPost("submit-comment")]
        public async Task<IActionResult> SubmitNewComment([FromBody] SubmitCommentDto newCommentDto)
        {
            BlogPostComment newComment = new BlogPostComment
            {
                BlogPostId  = newCommentDto.BlogPostId,
                Pseudonym   = newCommentDto.Pseudonym,
                Content     = newCommentDto.Content,
                CreatedAt   = DateTime.UtcNow,
                UpdatedAt   = DateTime.UtcNow
            };

            var (succeeded, error) = await _blogPostService.SubmitNewComment(newComment);
            if (succeeded)
            {
                return Ok(new { message = "Comment submitted successfully." });
            }
            else
            {
                return BadRequest(new { error });
            }
        }
    }
}
