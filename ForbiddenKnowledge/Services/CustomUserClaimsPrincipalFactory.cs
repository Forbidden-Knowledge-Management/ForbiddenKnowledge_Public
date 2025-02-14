using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

using ForbiddenKnowledge.Data.DbModels;

namespace ForbiddenKnowledge.Services
{
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User>
    {
        public CustomUserClaimsPrincipalFactory(UserManager<User> userManager, IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Set the correct case-sensitive pseudonym in the claim
            identity.RemoveClaim(identity.FindFirst(ClaimTypes.Name));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.OriginalPseudonym));

            // Add claim for lightweight vs full account
            string accountType = string.IsNullOrEmpty(user.Email) ? "Lightweight" : "Full";
            identity.AddClaim(new Claim("AccountType", accountType));

            return identity;
        }
    }
}
