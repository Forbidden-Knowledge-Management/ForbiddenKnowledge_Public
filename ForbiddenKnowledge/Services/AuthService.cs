using ForbiddenKnowledge.Data;
using ForbiddenKnowledge.Services.Interfaces;

namespace ForbiddenKnowledge.Services
{
    //The AuthService is a client side service for tracking authentication state.
    //It is a centralized place for handling the login/logout workflow for both full user accounts (which use Identity Core) and lighweight accounts (implemented separate from Identity)


    public class AuthService : IAuthService
    {
        private const string LightweightAccountCookieName = "fk_lightweight_user";

        public bool FullAccountAuthenticated { get; private set; } = false;
        public bool LightWeightAccountAuthenticated { get; private set; } = false;
        public string? Pseudonym { get; private set; }

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IUserService _userService;
        private readonly ForbiddenKnowledgeContext _forbiddenKnowledgeContext;

        public AuthService(IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IUserService userService, ForbiddenKnowledgeContext forbiddenKnowledgeContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
            _userService = userService;
            _forbiddenKnowledgeContext = forbiddenKnowledgeContext;
        }

        public async Task LoginFullAccountAsync(LoginModel loginModel)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/users/login", loginModel);
            if (response.IsSuccessStatusCode)
            {
                FullAccountAuthenticated = true;
                Pseudonym = loginModel.Pseudonym;
            }
            else
            {
                throw new Exception("Login failed. Check your pseudonym and password.");
            }
        }

        public async Task LogoutFullAccountAsync()
        {
            await _httpClient.PostAsync("api/users/logout", null);
            FullAccountAuthenticated = false;
            Pseudonym = null;
        }


        public async Task CreateLightweightAccountIdentityAsync()
        {
            Pseudonym = $"AnonymousUser{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            HttpResponse? response = _httpContextAccessor.HttpContext?.Response;
            response?.Cookies.Append(LightweightAccountCookieName, Pseudonym!, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = true,
                IsEssential = true
            });

            var (succeeded, errors) = await _userService.RegisterUserAsync(Pseudonym, null, null);
        }

        public async Task<bool> TryRetrieveLightweightAccountAsync()
        {
            var request = _httpContextAccessor.HttpContext?.Request;

            if (request?.Cookies.TryGetValue(LightweightAccountCookieName, out var pseudonym) == true)
            {
                if (_forbiddenKnowledgeContext.Users.Any(u => u.Pseudonym == pseudonym) == true)
                {
                    Pseudonym = pseudonym;
                    LightWeightAccountAuthenticated = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }


    }
}
