using ForbiddenKnowledge.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text.Json;

namespace ForbiddenKnowledge.Services
{
    //The AuthService is a client side service for tracking authentication state.
    //It is a centralized place for handling the login/logout workflow for both full user accounts (which use Identity Core) and lighweight accounts (implemented separate from Identity)


    public class AuthService : IAuthService
    {
        public string? Pseudonym { get; private set; }
        public bool IsLightweightUser { get; private set; }

        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(HttpClient httpClient, NavigationManager navigationManager, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;
            _authStateProvider = authStateProvider;
        }


        //ZM to-do
        //currently not authenticating
        //need to finds out why and refactor this method to return feedback
        public async Task<bool> CheckAuthenticationStatusAsync()
        {
            AuthenticationState authState = await _authStateProvider.GetAuthenticationStateAsync();
            ClaimsPrincipal user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                Pseudonym = user.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
                IsLightweightUser = user.HasClaim(c => c.Type == "AccountType" && c.Value == "Lightweight");
                return (true);
            }
            else
            {
                Pseudonym = null;
                IsLightweightUser = false;
                return (false);
            }
        }

        public async Task<(bool Succeeded, string? Error)> LoginFullAccountAsync(LoginModel loginModel)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"{_navigationManager.BaseUri}api/users/login", loginModel);
            if (response.IsSuccessStatusCode)
            {
                bool authenticated = await CheckAuthenticationStatusAsync();
                if (authenticated)
                {
                    return (true, null);
                }
                else
                {
                    return (false, "Authentication failed");
                }
            }
            else
            {
                string errorDetails;
                string responseContent = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
                    errorDetails = result["message"];
                    return (false, errorDetails);
                }
                else
                {
                    //handle BadRequest
                    if (!string.IsNullOrWhiteSpace(responseContent))
                    {
                        errorDetails = responseContent;
                    }
                    else
                    {
                        errorDetails = "Invalid request. Please check your input and try again.";
                    }
                    return (false, errorDetails);
                }
            }
        }

        //ZM to-do
        public async Task LogoutFullAccountAsync()
        {
            await _httpClient.PostAsync($"{_navigationManager.BaseUri}api/users/logout", null);
            await CheckAuthenticationStatusAsync();
        }


        public async Task<(bool Succeeded, string? Error)> CreateLightweightAccountIdentityAsync()
        {
            HttpResponseMessage response = await _httpClient.PostAsync($"{_navigationManager.BaseUri}api/users/lightweight-account", null);
            if (response.IsSuccessStatusCode)
            {
                //Note that lighweight accounts are automatically logged-in after creation.
                bool authenticated = await CheckAuthenticationStatusAsync();
                if (authenticated)
                {
                    return (true, null);
                }
                else
                {
                    return (false, "Lighweight account created, but subsequent authentication failed");
                }
            }
            else
            {
                string errorDetails;
                string responseContent = await response.Content.ReadAsStringAsync();

                //handle BadRequest
                if (!string.IsNullOrWhiteSpace(responseContent))
                {
                    errorDetails = responseContent;
                }
                else
                {
                    errorDetails = "Invalid request. Lightweight account not created. Please check your input and try again.";
                }
                return (false, errorDetails);
            }
        }

    }
}
