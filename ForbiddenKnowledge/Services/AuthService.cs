using ForbiddenKnowledge.Data;
using ForbiddenKnowledge.Data.DbModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using System.Reflection;
using System.Security.Claims;

namespace ForbiddenKnowledge.Services
{
    //The AuthService is a client side service for tracking authentication state.
    //It is a centralized place for handling the login/logout workflow for both full user accounts and lighweight accounts 

    /// <summary>
    /// The AuthService is a centralized place for managing user authentication state, via .NET Identity, for the frontend.
    /// Please note however this is server-side code, as is everything in Blazor server
    /// </summary>
    public class AuthService : IAuthService
    {
        public string? Pseudonym { get; private set; }
        public bool IsLightweightUser { get; private set; }

        private readonly ILogger _logger; 
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IUserService _userService;
        private readonly IJSRuntime _jSRuntime;

        public AuthService(IHttpContextAccessor httpContextAccessor, AuthenticationStateProvider authStateProvider, IUserService userService, ILogger<AuthService> logger, IJSRuntime jSRuntime)
        {
            _httpContextAccessor = httpContextAccessor;
            _authStateProvider = authStateProvider;
            _userService = userService;
            _logger = logger;
            _jSRuntime = jSRuntime;
        }


        public async Task<bool> CheckAuthenticationStatusAsync()
        {
            AuthenticationState authState = await _authStateProvider.GetAuthenticationStateAsync();
            ClaimsPrincipal user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                Pseudonym = user.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
                IsLightweightUser = user.HasClaim(c => c.Type == "AccountType" && c.Value == "Lightweight");
                _logger.LogInformation($"User {Pseudonym} has been authenticated.");
                return (true);
            }
            else
            {
                Pseudonym = null;
                IsLightweightUser = false;
                _logger.LogInformation($"User {Pseudonym} failed authentication.");
                return (false);
            }
        }

        public async Task<(bool Succeeded, string? Error)> LoginFullAccountAsync(LoginModel loginModel)
        {
            var (succeeded, error) = await _userService.LoginUserAsync(loginModel.UppercasedPseudonym, loginModel.Password);
            if (succeeded)
            {
                bool authenticated = await CheckAuthenticationStatusAsync();
                if (authenticated)
                {
                    _logger.LogInformation($"User {loginModel.Pseudonym} has successfully logged and authenticated via .NET Identity");
                    return (true, null);
                }
                else
                {
                    _logger.LogInformation($"User {loginModel.Pseudonym} has successfully logged, but failed subsequent authentication status check");
                    return (false, "Login succeeded, but subsequent authentication status check failed.");
                }
            }
            else
            {
                _logger.LogInformation($".NET Identity login failed for User {loginModel.Pseudonym}. Inner error: {error}");
                return (succeeded, $"Login failed: {error}");
            }
        }

        public async Task LogoutFullAccountAsync()
        {
            await _userService.LogoutUserAsync();
            await CheckAuthenticationStatusAsync();
            _logger.LogInformation($"A user has logged out.");
        }


        public async Task<(bool Succeeded, string? Error)> CreateLightweightAccount()
        {
            await _jSRuntime.InvokeVoidAsync("createAndLoginLightweightUserViaXHR");
            return (true, null);
        }

        public async Task<(bool Succeeeded, string? Error)> CreateFullAccount(RegisterModel registerModel)
        {
            var (createNewUserSucceeded, createNewUserErrors) = await _userService.CreateNewFullAccountWithIdentityAsync(registerModel.Pseudonym, registerModel.UppercasedPseudonym, registerModel.Email, registerModel.Password);
            if (createNewUserSucceeded)
            {
                //so the new user is now registered, but we now need to log them in
                LoginModel loginModel = new LoginModel
                {
                    Pseudonym = registerModel.UppercasedPseudonym,
                    Password = registerModel.Password
                };

                //Attempt automatic login after registration
                var (loginSucceeded, loginError) = await LoginFullAccountAsync(loginModel);
                if (loginSucceeded)
                {
                    _logger.LogInformation($"Full account {registerModel.Pseudonym} successfully created and authenticated.");
                    return (true, null);
                }
                else
                {
                    _logger.LogInformation($"Full account {registerModel.Pseudonym} successfully created, but login or subsequent authentication status check failed.");
                    return (false, $"Full account {registerModel.Pseudonym} successfully created, but login or subsequent authentication status check failed. Inner error: {loginError}");
                }
            }
            else
            {
                string errorsForLogs = string.Join("<br>", createNewUserErrors.Select(e => $"{e.Code}"));
                string errorsForFrontend = string.Join("<br>", createNewUserErrors.Select(e => $"{e.Description}"));

                _logger.LogInformation($"Full account {registerModel.Pseudonym} creation failed. Errors:\n{errorsForLogs.Replace("<br>", "\n")}");
                return (false, $"Full account {registerModel.Pseudonym} creation failed. Errors:<br>{errorsForFrontend}");
            }
        }

        public async Task<(bool Succeeded, string? Error)> RequestPasswordResetAsync(PasswordResetRequestModel passwordResetRequestModel)
        {
            if (string.IsNullOrWhiteSpace(passwordResetRequestModel.PseudonymOrEmail))
            {
                return (false, "Please provide either the Pseudonym or Email associated with your account.");
            }

            var (succeeded, error) = await _userService.RequestPasswordResetAsync(passwordResetRequestModel.PseudonymOrEmail);
            if (succeeded)
            {
                _logger.LogInformation($"{MethodBase.GetCurrentMethod().Name}: Password reset request made for {passwordResetRequestModel.PseudonymOrEmail}. Email sent.");
                return (true, $"Password reset request made for {passwordResetRequestModel.PseudonymOrEmail}. Email sent.");
            }
            else
            {
                _logger.LogInformation($"{MethodBase.GetCurrentMethod().Name}: Password reset request for {passwordResetRequestModel.PseudonymOrEmail} failed. Inner Error: {error}");
                return (false, $"Password reset request for {passwordResetRequestModel.PseudonymOrEmail} failed. Inner Error: {error}");
            }
        }

        public async Task<(bool Succeeded, string? Error)> PasswordResetAsync(PasswordResetModel passwordResetModel)
        {
            if (string.IsNullOrWhiteSpace(passwordResetModel.Pseudonym) || string.IsNullOrWhiteSpace(passwordResetModel.ResetCode) || string.IsNullOrWhiteSpace(passwordResetModel.Password))
            {

                return (false, "Please provide your pseudonym, the password reset code you received in an email, and the new password you would like to set for your account.");
            }

            var (succeeded, errors) = await _userService.ResetPasswordAsync(passwordResetModel.Pseudonym, passwordResetModel.ResetCode, passwordResetModel.Password);
            if (succeeded)
            {
                _logger.LogInformation($"{MethodBase.GetCurrentMethod().Name}: Password reset successful for {passwordResetModel.Pseudonym}.");
                return (true, null);
            }
            else
            {
                string errorsForLogs = string.Join("<br>", errors.Select(e => $"{e.Code}"));
                string errorsForFrontend = string.Join("<br>", errors.Select(e => $"{e.Description}"));

                _logger.LogError($"{MethodBase.GetCurrentMethod().Name}: Password reset for {passwordResetModel.Pseudonym} failed. Inner Error: {errorsForLogs}");
                return (false, $"Password reset for {passwordResetModel.Pseudonym} failed. Inner Error: {errorsForFrontend}");
            }

        }

    }
}
