using ForbiddenKnowledge.Data;

namespace ForbiddenKnowledge.Services.Interfaces
{
    public interface IAuthService
    {
        bool FullAccountAuthenticated { get;}
        bool LightWeightAccountAuthenticated { get;}
        string? Pseudonym { get;}

        Task LoginFullAccountAsync(LoginModel loginModel);

        Task LogoutFullAccountAsync();

        Task CreateLightweightAccountIdentityAsync();

        Task<bool> TryRetrieveLightweightAccountAsync();








    }
}
