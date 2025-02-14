using System.Net.Mail;
using Microsoft.AspNetCore.Identity;

using DnsClient;

using ForbiddenKnowledge.Data.DbModels;
using ForbiddenKnowledge.Data;
using Microsoft.EntityFrameworkCore;



namespace ForbiddenKnowledge.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ForbiddenKnowledgeContext _forbiddenKnowledgeContext;
        private readonly AuditDbContext _auditDbContext;
        private readonly ILogger _logger;

        public UserService(UserManager<User> userManager, SignInManager<User> signInManager, ForbiddenKnowledgeContext forbiddenKnowledgeContext, AuditDbContext auditDbContext, ILogger<UserService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _forbiddenKnowledgeContext = forbiddenKnowledgeContext;
            _auditDbContext = auditDbContext;
            _logger = logger;
        }

        /// <summary>
        /// This handles creating new Full user accounts and new Lightweight user accounts. Both are stored in the same "external_user" DB table.
        /// </summary>
        /// <param name="originalPseudonym"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<(bool Succeeded, IEnumerable<IdentityError> Errors)> RegisterUserAsync(string originalPseudonym, string uppercasedPseudonym, string? email = null, string? password = null)
        {
            var pseudonymUniquenessCheckResult = CheckPseudonymUniqueness(uppercasedPseudonym);
            if (pseudonymUniquenessCheckResult.Result.Succeeded == false)
            {
                return (false, pseudonymUniquenessCheckResult.Result.Errors);
            }
            else
            {
                User user = new User
                {
                    OriginalPseudonym = originalPseudonym,
                    UppercasedPseudonym = uppercasedPseudonym,
                    Email = email,
                    CreatedAt = DateTime.UtcNow
                };

                IdentityResult result;
                if (string.IsNullOrEmpty(password))
                {
                    //Lighweight account, still created and managed by .NET Identity.
                    result = await _userManager.CreateAsync(user);

                    //for lightweight accounts, it is very important that we also sign the user in.
                    //When the user is signed in using the .NET Identity method below, it will automatically create a cookie so that the lighweight user can be authenticated.
                    //because the cookie is all they have, no password or email.
                    await _signInManager.SignInAsync(user, isPersistent: true);
                }
                else
                {
                    //For Full accounts, we need to validate the email address.
                    //.NET Identity will automatically validate the password based on rules configured in Program.cs.
                    var emailValidationResult = ValidateEmail(email);
                    if (emailValidationResult.Result.Succeeded == false)
                    {
                        return (false, emailValidationResult.Result.Errors);
                    }
                    result = await _userManager.CreateAsync(user, password);
                }

                return (result.Succeeded, result.Errors);
            }
        }

        public async Task<(bool Succeeded, string? Error)> LoginUserAsync(string uppercasedPseudonym, string password)
        {
            User user = await _userManager.FindByNameAsync(uppercasedPseudonym);
            if (user == null)
            {
                return (false, "Pseudonym not found");
            }

            SignInResult signInResult = await _signInManager.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: false);
            _logger.LogInformation("SignInResult: {Result}", signInResult.Succeeded);
            if (signInResult.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: true);
                return (true, null);
            }

            string errorMessage = signInResult.IsLockedOut ? "Account is locked."
                  : signInResult.IsNotAllowed ? "Login not allowed."
                  : signInResult.RequiresTwoFactor ? "Two-factor authentication required."
                  : "Invalid password.";

            return (signInResult.Succeeded, errorMessage);
        }

        public async Task LogoutUserAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<bool> IsRateLimitedForLightweightAccounts(string ipAddress)
        {
            DateTime oneHourAgo = DateTime.UtcNow.AddHours(-1);

            var recentAttempts = await _auditDbContext.AuditTrails
                                       .CountAsync(a => a.RequestUrl.Contains("/lightweight-account") &&
                                        a.Timestamp >= oneHourAgo &&
                                        a.IpAddress == ipAddress);

            return recentAttempts >= 3; // Limit to 3 per hour
        }

        //Pseudonyms are case-insensitive. That is maintained elsewhere. This is for more specific, custom stuff.
        //ZM to-do: this is just an early attempt. Should try to improve this.
        private async Task<(bool Succeeded, IEnumerable<IdentityError> Errors)> CheckPseudonymUniqueness(string uppercasedPseudonym)
        {
            List<IdentityError> Errors = new List<IdentityError>();

            if (uppercasedPseudonym.StartsWith("ANONYMOUSUSER"))
            {
                //lighweight account pseudonyms are always unique because they are appended with a GUID
                return (true, Errors);
            }

            if (_forbiddenKnowledgeContext.Users.Any(u => u.UppercasedPseudonym == uppercasedPseudonym) == true)
            {
                Errors.Add(new IdentityError() { Code = "Literal Pseudonym Match", Description = "A user with this exact pseudonym already exists" });
                return (false, Errors);
            }

            //should add more fuzzy logic checks


            return (true, Errors);
        }

        /// <summary>
        /// Checks user-provided email address to ensure it exists, it has a valid format, it has a queryable MX record, and it is unique among FK users.
        /// The uniqueness restriction is just to try to deter people from creating multiple accounts.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private async Task<(bool Succeeded, IEnumerable<IdentityError> Errors)> ValidateEmail(string? email)
        {
            List<IdentityError> errors = new List<IdentityError>();

            if (string.IsNullOrEmpty(email))
            {
                errors.Add(new IdentityError() { Code = "Null Email", Description = "An email address must be provided" });
                return (false, errors);
            }

            try
            {
                MailAddress mailAddress = new MailAddress(email);
            }
            catch
            {
                errors.Add(new IdentityError() { Code = "Invalid Email", Description = "The email address provided has an invalid format" });
                return (false, errors);
            }

            string domain = email.Split('@').Last();
            if (await DomainHasMxRecordAsync(domain) == false)
            {
                errors.Add(new IdentityError { Code = "Issue with Email MX record", Description = "The email domain does not have an MX DNS record" });
                return (false, errors);
            }

            if (_forbiddenKnowledgeContext.Users.Any(u => u.Email == email) == true)
            {
                errors.Add(new IdentityError() { Code = "Email Not Unique", Description = "A user with this exact email already exists" });
                return (false, errors);
            }

            return (true, errors);
        }

        private async Task<bool> DomainHasMxRecordAsync(string domain)
        {
            //This is in a try-catch block because DNS queries can fail for a variety of reasons.
            //Practically speaking, this is a quick and easy way to test
            // 1. The domain exists
            // 2. The domain's DNS records are queryable
            // 3. The domain has an MX record
            try
            {
                LookupClient lookupClient = new LookupClient();
                var result = await lookupClient.QueryAsync(domain, QueryType.MX);
                return result.Answers.MxRecords().Any();
            }
            catch
            {
                return false;
            }
        }
    }
}
