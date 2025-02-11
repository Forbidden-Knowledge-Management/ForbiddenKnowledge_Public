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
        private readonly IUserService _userService;

        private const string LightweightAccountCookieName = "fk_lightweight_user";

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

            var (succeeded, errors) = await _userService.RegisterUserAsync(registerModel.Pseudonym, registerModel.Email, registerModel.Password);

            if (succeeded)
            {
                return Ok(new { Message = "Registration successful." });
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

            var (succeeded, error) = await _userService.LoginUserAsync(loginModel.Pseudonym, loginModel.Password);

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

            string pseudonym = $"ANONYMOUSUSER{Guid.NewGuid().ToString("N").Substring(0, 8)}".ToUpperInvariant();
            var (succeeded, errors) = await _userService.RegisterUserAsync(pseudonym, null, null);

            if (!succeeded)
            {
                return BadRequest(errors);
            }

            Response?.Cookies.Append(LightweightAccountCookieName, pseudonym!, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = true,
                IsEssential = true
            });

            return Ok(new { Pseudonym = pseudonym });
        }

    }
}
