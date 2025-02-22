using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

using ForbiddenKnowledge.Data;
using ForbiddenKnowledge.Data.DbModels;
using ForbiddenKnowledge.Services;


namespace ForbiddenKnowledge.Controllers
{
    [Route("api/login")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public LoginController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<LoginController> logger, IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
        }

        [HttpPost("generate-login-cookie-full")]
        public async Task<IActionResult> GenerateLoginCookieForFullAccounts([FromBody] LoginModel loginModel)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Error in {MethodBase.GetCurrentMethod().Name}:  ModelState is not valid.");
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(loginModel.Pseudonym) || string.IsNullOrWhiteSpace(loginModel.Password))
            {
                _logger.LogWarning($"{MethodBase.GetCurrentMethod().Name}: user Pseudonym and/or password were not provided.");
                return BadRequest("Pseudonym and password are required.");
            }

            User user = await _userManager.FindByNameAsync(loginModel.Pseudonym);
            if (user == null || user == default)
            {
                _logger.LogWarning($"{MethodBase.GetCurrentMethod().Name}: Invalid login attempt. User Pseudonym does not exist.");
                return Unauthorized("Invalid login attempt. Pseudonym does not exist.");
            }

            //Check if the user is locked out
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning($"{MethodBase.GetCurrentMethod().Name}: {loginModel.Pseudonym} has been locked due to too many failed login attempts.");
                return StatusCode(423, $"Your account has been locked due to too many failed login attempts. The lockout will end in 1 hour.");
            }

            Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager.PasswordSignInAsync(user, loginModel.Password, isPersistent: true, lockoutOnFailure: true);
            if (signInResult.Succeeded)
            {
                //Reset failed attempts after successful login
                await _userManager.ResetAccessFailedCountAsync(user);
                _logger.LogInformation($"{MethodBase.GetCurrentMethod().Name}: {loginModel.Pseudonym} succeessfully logged in.");
                return Ok();
            }

            string errorMessage = signInResult.IsLockedOut ? "Your account has been locked due to too many failed login attempts. The lockout will end in 1 hour."
                  : signInResult.IsNotAllowed ? "Login not allowed."
                  : signInResult.RequiresTwoFactor ? "Two-factor authentication required."
                  : "Incorrect password.";

            if (signInResult.IsLockedOut)
            {
                return StatusCode(423, $"Your account has been locked due to too many failed login attempts. The lockout will end in 1 hour.");
            }

            _logger.LogWarning($"{MethodBase.GetCurrentMethod().Name}: Invalid login attempt for {loginModel.Pseudonym}. {errorMessage}.");
            return Unauthorized(new { message = $"Invalid login attempt: {errorMessage}" });
        }


        [HttpPost("generate-login-cookie-lighweight")]
        public async Task<IActionResult> GenerateLoginCookieForLightweightAccounts()
        {
            string ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            // Check rate limit using audit logs
            if (await _userService.IsRateLimitedForLightweightAccounts(ipAddress))
            {
                _logger.LogWarning($"{MethodBase.GetCurrentMethod().Name}: IP Address {ipAddress} was blocked from creating a lightweight account due to hitting the rate limit.");
                return StatusCode(429, "Too many lightweight accounts have been created from this IP address recently. Try again later.");
            }

            string pseudonym = $"AnonymousUser{Guid.NewGuid().ToString("N").Substring(0, 8)}";

            User user = new User
            {
                OriginalPseudonym = pseudonym,
                UppercasedPseudonym = pseudonym.ToUpperInvariant(),
                Email = null,
                CreatedAt = DateTime.UtcNow
            };

            //create Lightweight account with no password. Still created and managed by .NET Identity.
            IdentityResult result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogWarning($"{MethodBase.GetCurrentMethod().Name}: An internal error occurred while attempting to create lightweight account {pseudonym}.");
                return StatusCode(500, $"An internal error occurred while attempting to create lightweight account {pseudonym}.");
            }

            await _signInManager.SignInAsync(user, isPersistent: true);

            _logger.LogInformation($"{MethodBase.GetCurrentMethod().Name}: Lightweight user {pseudonym} succeessfully created and logged in.");
            return Ok();
        }

    }
}
