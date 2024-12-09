using ForbiddenKnowledge.Data.DbModels;
using ForbiddenKnowledge.Services;
using ForbiddenKnowledge.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;



namespace ForbiddenKnowledge.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (succeeded, errors) = await _userService.RegisterUserAsync(model.Pseudonym, model.Email, model.Password);

            if (succeeded)
            {
                return Ok(new { Message = "Registration successful." });
            }

            return BadRequest(errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (succeeded, error) = await _userService.LoginUserAsync(model.Pseudonym, model.Password);

            if (succeeded)
            {
                return Ok(new {Message = "Login successful"});
            }

            return Unauthorized(new { Message = $"Invalid login attempt: {error}" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutUserAsync();
            return Ok(new { Message = "Logout successful"});
        }

    }
}
