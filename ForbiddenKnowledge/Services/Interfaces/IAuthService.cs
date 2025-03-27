using ForbiddenKnowledge.Data;
using ForbiddenKnowledge.Data.DbModels;

namespace ForbiddenKnowledge.Services
{
    public interface IAuthService
    {
        User appUser { get; set; }

        Task<bool> CheckAuthenticationStatusAsync();

        Task<(bool Succeeded, string? Error)> LoginFullAccountAsync(LoginModel loginModel);

        Task LogoutFullAccountAsync();

        Task<(bool Succeeded, string? Error)> CreateLightweightAccount();

        Task<(bool Succeeeded, string? Error)> CreateFullAccount(RegisterModel registerModel);

        Task<(bool Succeeded, string? Error)> RequestPasswordResetAsync(PasswordResetRequestModel passwordResetRequestModel);

        Task<(bool Succeeded, string? Error)> PasswordResetAsync(PasswordResetModel passwordResetModel);

    }
}
