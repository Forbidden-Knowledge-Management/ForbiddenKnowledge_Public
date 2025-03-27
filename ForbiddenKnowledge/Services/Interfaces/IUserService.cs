using ForbiddenKnowledge.Data.DbModels;
using Microsoft.AspNetCore.Identity;

namespace ForbiddenKnowledge.Services
{
    public interface IUserService
    {
        Task<User?> GetUserById(string userId);

        Task<(bool Succeeded, IEnumerable<IdentityError> Errors)> CreateNewFullAccountWithIdentityAsync(string originalPseudonym, string uppercasedPseudonym, string? email = null, string? password = null);

        Task<(bool Succeeded, string? Error)> LoginUserAsync(string uppercasedPseudonym, string password);

        Task LogoutUserAsync();

        Task<(bool Succeeded, string? Error)> RequestPasswordResetAsync(string pseudonymOrEmail);

        Task<bool> IsRateLimitedForLightweightAccounts(string ipAddress);

        Task<(bool Succeeded, IEnumerable<IdentityError> Errors)> ResetPasswordAsync(string pseudonym, string resetCode, string newPassword);

        DateTime ConvertDateTimeToUsersTimeZone(DateTime dateTime, string timeZoneId);




    }
}
