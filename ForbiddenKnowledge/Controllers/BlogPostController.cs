using Microsoft.AspNetCore.Mvc;

using ForbiddenKnowledge.Data.Models;
using ForbiddenKnowledge.Services;


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



    }
}
