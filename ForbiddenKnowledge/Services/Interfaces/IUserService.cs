using Microsoft.AspNetCore.Identity;

namespace ForbiddenKnowledge.Services
{
    public interface IUserService
    {
        Task<(bool Succeeded, IEnumerable<IdentityError> Errors)> CreateNewFullAccountWithIdentityAsync(string originalPseudonym, string uppercasedPseudonym, string? email = null, string? password = null);

        Task<(bool Succeeded, string? Error)> LoginUserAsync(string uppercasedPseudonym, string password);

        Task LogoutUserAsync();

        Task<bool> IsRateLimitedForLightweightAccounts(string ipAddress);












    }
}
