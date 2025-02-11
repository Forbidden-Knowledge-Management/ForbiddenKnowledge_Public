using ForbiddenKnowledge.Data;

namespace ForbiddenKnowledge.Services
{
    public interface IAuthService
    {
        bool FullAccountAuthenticated { get;}
        bool LightWeightAccountAuthenticated { get;}
        string? Pseudonym { get;}

        Task<(bool Succeeded, string? Error)> LoginFullAccountAsync(LoginModel loginModel);

        Task LogoutFullAccountAsync();

        Task CreateLightweightAccountIdentityAsync();

        Task TryRetrieveLightweightAccountAsync();








    }
}
