using Microsoft.AspNetCore.Identity;

using ForbiddenKnowledge.Data.DbModels;
using ForbiddenKnowledge.Data;


namespace ForbiddenKnowledge.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ForbiddenKnowledgeContext _forbiddenKnowledgeContext;

        public UserService(UserManager<User> userManager, SignInManager<User> signInManager, ForbiddenKnowledgeContext forbiddenKnowledgeContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _forbiddenKnowledgeContext = forbiddenKnowledgeContext;
        }

        //ZM to-do: ensure uniqueness of pseudonym
        public async Task<(bool Succeeded, IEnumerable<IdentityError> Errors)> RegisterUserAsync(string pseudonym, string? email = null, string? password = null)
        {
            var pseudonymUniquenessCheckResult = CheckPseudonymUniqueness(pseudonym);

            if (pseudonymUniquenessCheckResult.Result.Succeeded == false)
            {
                return (false, pseudonymUniquenessCheckResult.Result.Errors);
            }
            else
            {
                var user = new User
                {
                    Pseudonym = pseudonym,
                    Email = email,
                    CreatedAt = DateTime.UtcNow
                };

                IdentityResult result = await _userManager.CreateAsync(user, password);
                return (result.Succeeded, result.Errors);
            }
        }

        public async Task<(bool Succeeded, string? Error)> LoginUserAsync(string pseudonym, string password)
        {
            var user = await _userManager.FindByNameAsync(pseudonym);
            if (user == null)
            {
                return (false, "Pseudonym not found");
            }

            SignInResult result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: false);
            return (result.Succeeded, "Login error");
        }

        public async Task LogoutUserAsync()
        {
            await _signInManager.SignOutAsync();
        }

        //ZM to-do: this is just an early attempt. Should try to improve this.
        private async Task<(bool Succeeded, IEnumerable<IdentityError> Errors)> CheckPseudonymUniqueness(string pseudonym)
        {
            List<IdentityError> Errors = new List<IdentityError>();

            if (pseudonym.StartsWith("AnonymousUser"))
            {
                //lighweight account pseudonyms are always unique because they are appended with a GUID
                return (true, Errors);
            }

            if (_forbiddenKnowledgeContext.Users.Any(u => u.Pseudonym == pseudonym) == true)
            {
                Errors.Add(new IdentityError() { Code = "Literal Match" , Description = "A user with this exact pseudonym already exists" });
                return (false, Errors);
            }

            //should add more fuzzy logic checks


            return (true, Errors);
        }
    }
}
