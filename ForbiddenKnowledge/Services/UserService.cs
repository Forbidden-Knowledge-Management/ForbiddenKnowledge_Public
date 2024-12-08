using Microsoft.AspNetCore.Identity;

using ForbiddenKnowledge.Data.DbModels;
using ForbiddenKnowledge.Services.Interfaces;


namespace ForbiddenKnowledge.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UserService(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        //ZM to-do: ensure uniqueness of pseudonym
        public async Task<(bool Succeeded, IEnumerable<IdentityError> Errors)> RegisterUserAsync(string pseudonym, string email, string password)
        {
            var user = new User
            {
                Pseudonym   = pseudonym,
                Email       = email,
                CreatedAt   = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            return (result.Succeeded, result.Errors);
        }

        public async Task<(bool Succeeded, string? Pseudonym)> LoginUserAsync(string pseudonym, string password)
        {
            var user = await _userManager.FindByNameAsync(pseudonym);
            if (user == null)
            {
                return (false, null);
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: false);
            return (result.Succeeded, result.Succeeded ? user.Pseudonym : null);
        }

        public async Task LogoutUserAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
