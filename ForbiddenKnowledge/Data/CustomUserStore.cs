using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

using ForbiddenKnowledge.Data.DbModels;
using System.Data.Common;

namespace ForbiddenKnowledge.Data
{
    public class CustomUserStore : IUserStore<User>, IUserPasswordStore<User>
    {
        private readonly ForbiddenKnowledgeContext _forbiddenKnowledgeContext;


        public CustomUserStore(ForbiddenKnowledgeContext forbiddenKnowledgeContext)
        {
            _forbiddenKnowledgeContext = forbiddenKnowledgeContext;
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
                        .FirstOrDefaultAsync(u => u.Pseudonym.ToUpperInvariant() == name.ToUpperInvariant(), cancellationToken);
        }

        public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return await _forbiddenKnowledgeContext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Email.ToUpperInvariant() == email.ToUpperInvariant(), cancellationToken);
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Pseudonym);
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Pseudonym.ToUpperInvariant());
        }

        //ZM to-do: modify this to ensure it is unique
        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            user.Pseudonym = userName;
            return Task.CompletedTask;
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            // Not needed as NormalizedEmail is computed dynamically
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                await _forbiddenKnowledgeContext.Users.AddAsync(user, cancellationToken);
                await _forbiddenKnowledgeContext.SaveChangesAsync(cancellationToken);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
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
