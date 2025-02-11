using ForbiddenKnowledge.Data;
using Microsoft.AspNetCore.Components;

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
        private readonly ForbiddenKnowledgeContext _forbiddenKnowledgeContext;
        private readonly NavigationManager _navigationManager;

        public AuthService(IHttpContextAccessor httpContextAccessor, HttpClient httpClient, ForbiddenKnowledgeContext forbiddenKnowledgeContext, NavigationManager navigationManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
            _forbiddenKnowledgeContext = forbiddenKnowledgeContext;
            _navigationManager = navigationManager;
        }

        public async Task<(bool Succeeded, string? Error)> LoginFullAccountAsync(LoginModel loginModel)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"{_navigationManager.BaseUri}api/users/login", loginModel);
            if (response.IsSuccessStatusCode)
            {
                FullAccountAuthenticated = true;
                Pseudonym = loginModel.Pseudonym;
                return (true, null);
            }
            else
            {
                string errorDetails = await response.Content.ReadAsStringAsync();
                return (false, errorDetails);
            }
        }

        public async Task LogoutFullAccountAsync()
        {
            await _httpClient.PostAsync($"{_navigationManager.BaseUri}api/users/logout", null);
            FullAccountAuthenticated = false;
            Pseudonym = null;
        }


        public async Task CreateLightweightAccountIdentityAsync()
        {
            HttpResponseMessage lighweightAccountCreationResponse = await _httpClient.PostAsync($"{_navigationManager.BaseUri}api/users/lightweight-account", null);
            if (lighweightAccountCreationResponse.IsSuccessStatusCode)
            {
                var result = await lighweightAccountCreationResponse.Content.ReadFromJsonAsync<LightweightAccountResponse>();
                Pseudonym = result.Pseudonym;
                LightWeightAccountAuthenticated = true;
            }
            else
            {
                throw new Exception("Lightweight account creation failed due to rate-limiting or another error.");
            }
        }


        public async Task TryRetrieveLightweightAccountAsync()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request?.Cookies.TryGetValue(LightweightAccountCookieName, out var pseudonym) == true)
            {
                if (_forbiddenKnowledgeContext.Users.Any(u => u.Pseudonym == pseudonym) == true)
                {
                    Pseudonym = pseudonym;
                    LightWeightAccountAuthenticated = true;

                }
                else
                {
                    LightWeightAccountAuthenticated = false;
                }
            }
            else
            {
                LightWeightAccountAuthenticated = false;
            }
        }


        private class LightweightAccountResponse
        {
            public string Pseudonym { get; set; }
        }

    }
}
