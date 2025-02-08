using ForbiddenKnowledge.Data;

namespace ForbiddenKnowledge.Services
{
    public interface IAuthService
    {
        bool FullAccountAuthenticated { get;}
        bool LightWeightAccountAuthenticated { get;}
        string? Pseudonym { get;}

        Task LoginFullAccountAsync(LoginModel loginModel);

        Task LogoutFullAccountAsync();

        Task CreateLightweightAccountIdentityAsync();

        Task TryRetrieveLightweightAccountAsync();








    }
}
