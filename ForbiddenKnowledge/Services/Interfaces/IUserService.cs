using Microsoft.AspNetCore.Identity;

namespace ForbiddenKnowledge.Services.Interfaces
{
    public interface IUserService
    {
        Task<(bool Succeeded, IEnumerable<IdentityError> Errors)> RegisterUserAsync(string pseudonym, string email, string password);

        Task<(bool Succeeded, string? Error)> LoginUserAsync(string pseudonym, string password);

        Task LogoutUserAsync();









    }
}
