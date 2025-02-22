using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

using ForbiddenKnowledge.Data.DbModels;
using Microsoft.EntityFrameworkCore.Internal;


namespace ForbiddenKnowledge.Data
{
    public class CustomUserStore : IUserStore<User>, IUserPasswordStore<User>, IUserSecurityStampStore<User>, IUserLockoutStore<User>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CustomUserStore> _logger;


        public CustomUserStore(IServiceProvider serviceProvider, ILogger<CustomUserStore> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        private ForbiddenKnowledgeContext CreateDbContext()
        {
            var scope = _serviceProvider.CreateScope(); // Create a new DI scope
            return scope.ServiceProvider.GetRequiredService<ForbiddenKnowledgeContext>(); // Get a new scoped DbContext
        }

        //Please note that this method in the IUserStore interface is defined with a string as the first param
        public async Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            //Use a NEW DbContext instance to avoid concurrency issues
            using ForbiddenKnowledgeContext forbiddenKnowledgeContext = CreateDbContext();

            if (!int.TryParse(userId, out var id)) return null;
            return await forbiddenKnowledgeContext.Users.FindAsync(id, cancellationToken);
        }

        public async Task<User?> FindByNameAsync(string name, CancellationToken cancellationToken)
        {
            //Use a NEW DbContext instance to avoid concurrency issues
            using ForbiddenKnowledgeContext forbiddenKnowledgeContext = CreateDbContext();

            return await forbiddenKnowledgeContext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UppercasedPseudonym == name.ToUpper(), cancellationToken);
        }

        public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken)
        {
            //Use a NEW DbContext instance to avoid concurrency issues
            using ForbiddenKnowledgeContext forbiddenKnowledgeContext = CreateDbContext();

            return await forbiddenKnowledgeContext.Users
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
                //When  is called to create full accounts, .NET Identity will already execute an INSERT command against the DB before this method even runs.
                //so, if we attempt to add the new user to the DB ourselves via EF, we will get a PK violation because it is a duplicate.
                //However, the when _userManager.CreateAsync(user) is called (no password) for lightweight accounts, .NET Identity does NOT make the DB record automatically.
                //So we need to deal with that ourselves
                if (user.Email == null && user.PasswordHash == null && user.UppercasedPseudonym.StartsWith("ANONYMOUSUSER"))
                {
                    //Use a NEW DbContext instance to avoid concurrency issues
                    using ForbiddenKnowledgeContext forbiddenKnowledgeContext = CreateDbContext();

                    forbiddenKnowledgeContext.Users.Add(user);
                    await forbiddenKnowledgeContext.SaveChangesAsync(cancellationToken);
                    return IdentityResult.Success;
                }
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{MethodBase.GetCurrentMethod().Name}: Error creating user {user.OriginalPseudonym}");
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                //Use a NEW DbContext instance to avoid concurrency issues
                using ForbiddenKnowledgeContext forbiddenKnowledgeContext = CreateDbContext();

                forbiddenKnowledgeContext.Users.Update(user);
                await forbiddenKnowledgeContext.SaveChangesAsync(cancellationToken);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                //Use a NEW DbContext instance to avoid concurrency issues
                using ForbiddenKnowledgeContext forbiddenKnowledgeContext = CreateDbContext();

                forbiddenKnowledgeContext.Users.Remove(user);
                await forbiddenKnowledgeContext.SaveChangesAsync(cancellationToken);
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

            //Use a NEW DbContext instance to avoid concurrency issues
            using ForbiddenKnowledgeContext forbiddenKnowledgeContext = CreateDbContext();

            forbiddenKnowledgeContext.Users.Update(user);
            await forbiddenKnowledgeContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<string> GetSecurityStampAsync(User user, CancellationToken cancellationToken)
        {
            //Use a NEW DbContext instance to avoid concurrency issues
            using ForbiddenKnowledgeContext forbiddenKnowledgeContext = CreateDbContext();

            return await forbiddenKnowledgeContext.Users
                .Where(u => u.Id == user.Id)
                .Select(u => u.SecurityStamp)
                .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
        }

        public async Task SetSecurityStampAsync(User user, string stamp, CancellationToken cancellationToken)
        {
            user.SecurityStamp = stamp;

            //Use a NEW DbContext instance to avoid concurrency issues
            using ForbiddenKnowledgeContext forbiddenKnowledgeContext = CreateDbContext();
            forbiddenKnowledgeContext.Users.Update(user);
            await forbiddenKnowledgeContext.SaveChangesAsync(cancellationToken);
        }


        public Task<bool> GetLockoutEnabledAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.LockoutEnabled);
        }

        public async Task SetLockoutEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
        {
            user.LockoutEnabled = enabled;

            //Use a NEW DbContext instance to avoid concurrency issues
            using ForbiddenKnowledgeContext forbiddenKnowledgeContext = CreateDbContext();

            forbiddenKnowledgeContext.Users.Update(user);
            await forbiddenKnowledgeContext.SaveChangesAsync(cancellationToken);
        }

        public Task<int> GetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        public async Task SetAccessFailedCountAsync(User user, int count, CancellationToken cancellationToken)
        {
            user.AccessFailedCount = count;

            //Use a NEW DbContext instance to avoid concurrency issues
            using ForbiddenKnowledgeContext forbiddenKnowledgeContext = CreateDbContext();

            forbiddenKnowledgeContext.Users.Update(user);
            await forbiddenKnowledgeContext.SaveChangesAsync(cancellationToken);
        }

        public async Task ResetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            user.AccessFailedCount = 0;

            //Use a NEW DbContext instance to avoid concurrency issues
            using ForbiddenKnowledgeContext forbiddenKnowledgeContext = CreateDbContext();

            forbiddenKnowledgeContext.Users.Update(user);
            await forbiddenKnowledgeContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<DateTimeOffset?> GetLockoutEndDateAsync(User user, CancellationToken cancellationToken)
        {
            return user.LockoutEnd;
        }

        public async Task SetLockoutEndDateAsync(User user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            user.LockoutEnd = lockoutEnd?.UtcDateTime;

            //Use a NEW DbContext instance to avoid concurrency issues
            using ForbiddenKnowledgeContext forbiddenKnowledgeContext = CreateDbContext();

            forbiddenKnowledgeContext.Users.Update(user);
            await forbiddenKnowledgeContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> IncrementAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            user.AccessFailedCount++;

            //Use a NEW DbContext instance to avoid concurrency issues
            using ForbiddenKnowledgeContext forbiddenKnowledgeContext = CreateDbContext();

            forbiddenKnowledgeContext.Users.Update(user);
            await forbiddenKnowledgeContext.SaveChangesAsync(cancellationToken);
            return user.AccessFailedCount;
        }



        public void Dispose()
        {
            // Do NOT dispose of _forbiddenKnowledgeContext manually.
            // The DI container will handle it when the scope ends.
        }

    }
}
