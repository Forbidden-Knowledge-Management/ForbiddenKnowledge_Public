using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using ForbiddenKnowledge.Data.DbModels;

namespace ForbiddenKnowledge.Data
{
    public class CustomUserStore : IUserStore<User>, IUserPasswordStore<User>
    {
        private readonly ForbiddenKnowledgeContext _forbiddenKnowledgeContext;
        private readonly ILogger<CustomUserStore> _logger;


        public CustomUserStore(ForbiddenKnowledgeContext forbiddenKnowledgeContext, ILogger<CustomUserStore> logger)
        {
            _forbiddenKnowledgeContext = forbiddenKnowledgeContext;
            _logger = logger;
        }

        //Please note that this method in the IUserStore interface is defined with a string as the first param
        public async Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            if (!int.TryParse(userId, out var id)) return null;
            return await _forbiddenKnowledgeContext.Users.FindAsync(id, cancellationToken);
        }

        public async Task<User?> FindByNameAsync(string name, CancellationToken cancellationToken)
        {
            return await _forbiddenKnowledgeContext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UppercasedPseudonym == name.ToUpper(), cancellationToken);
        }

        public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return await _forbiddenKnowledgeContext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.OriginalPseudonym);
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UppercasedPseudonym);
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            user.OriginalPseudonym = userName;
            return Task.CompletedTask;
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            user.UppercasedPseudonym = normalizedName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                //Weird situation here.
                //When _userManager.CreateAsync() is called, .NET Identity will already execute an INSERT command against the DB before this method even runs.
                //so, if we attempt to add the new user to the DB ourselves via EF, we will get a PK violation because it is a duplicate.
                //However, the IUserStore interface requires us to have a CreateAsync method
                //So we simply just return a success result here.
                _logger.LogInformation("CreateAsync in CustomUserStore called, but skipping manual insertion due to .NET Identity behavior.");
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Pseudonym}", user.OriginalPseudonym);
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                _forbiddenKnowledgeContext.Users.Update(user);
                _forbiddenKnowledgeContext.SaveChangesAsync(cancellationToken);
                return Task.FromResult(IdentityResult.Success);
            }
            catch (Exception ex)
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError { Description = ex.Message }));
            }
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                _forbiddenKnowledgeContext.Users.Remove(user);
                await _forbiddenKnowledgeContext.SaveChangesAsync(cancellationToken);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            // Return true if the user has a non-empty password hash
            return Task.FromResult(!string.IsNullOrEmpty(user?.PasswordHash));
        }

        public Task<string?> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public async Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            _forbiddenKnowledgeContext.Users.Update(user);
            await _forbiddenKnowledgeContext.SaveChangesAsync(cancellationToken);
        }




        public void Dispose()
        {
            _forbiddenKnowledgeContext.Dispose();
        }

    }
}
