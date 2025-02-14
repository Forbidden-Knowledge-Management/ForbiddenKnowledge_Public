using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using ForbiddenKnowledge.Data.DbModels;
using ForbiddenKnowledge.Services;
using ForbiddenKnowledge.Data;


namespace ForbiddenKnowledge.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (succeeded, errors) = await _userService.RegisterUserAsync(registerModel.Pseudonym, registerModel.UppercasedPseudonym, registerModel.Email, registerModel.Password);

            if (succeeded)
            {
                return Ok(new { message = "Registration successful." });
            }

            return BadRequest(errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (succeeded, error) = await _userService.LoginUserAsync(loginModel.UppercasedPseudonym, loginModel.Password);

            if (succeeded)
            {
                return Ok(new { message = "Login successful"});
            }

            return Unauthorized(new { message = $"Invalid login attempt: {error}" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutUserAsync();
            return Ok(new { message = "Logout successful"});
        }


        [HttpPost("lightweight-account")]
        public async Task<IActionResult> CreateLightweightAccount()
        {
            HttpContext httpContext = HttpContext;
            string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            // Check rate limit using audit logs
            if (await _userService.IsRateLimitedForLightweightAccounts(ipAddress))
            {
                return BadRequest("Too many lightweight accounts created from this IP. Try again later.");
            }

            string pseudonym = $"Anonymoususer{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            var (succeeded, errors) = await _userService.RegisterUserAsync(pseudonym, pseudonym.ToUpperInvariant(), null, null);

            if (!succeeded)
            {
                return BadRequest(errors);
            }

            return Ok(new { message = "Registration successful." });
        }
    }
}
