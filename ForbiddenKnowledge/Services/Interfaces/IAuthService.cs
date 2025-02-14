using ForbiddenKnowledge.Data;

namespace ForbiddenKnowledge.Services
{
    public interface IAuthService
    {
        string? Pseudonym { get;}

        Task<bool> CheckAuthenticationStatusAsync();

        Task<(bool Succeeded, string? Error)> LoginFullAccountAsync(LoginModel loginModel);

        Task LogoutFullAccountAsync();

        Task<(bool Succeeded, string? Error)> CreateLightweightAccountIdentityAsync();









    }
}
