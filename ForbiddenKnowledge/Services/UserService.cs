using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.Reflection;
using System.Net;

using DnsClientX;

using ForbiddenKnowledge.Data.DbModels;
using ForbiddenKnowledge.Data;



namespace ForbiddenKnowledge.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ForbiddenKnowledgeContext _forbiddenKnowledgeContext;
        private readonly AuditDbContext _auditDbContext;
        private readonly ILogger _logger;
        private readonly IJSRuntime _jSRuntime;

        public UserService(UserManager<User> userManager, SignInManager<User> signInManager, ForbiddenKnowledgeContext forbiddenKnowledgeContext, AuditDbContext auditDbContext, ILogger<UserService> logger, IJSRuntime jSRuntime)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _forbiddenKnowledgeContext = forbiddenKnowledgeContext;
            _auditDbContext = auditDbContext;
            _logger = logger;
            _jSRuntime = jSRuntime;
        }

        /// <summary>
        /// This handles creating new Full user accounts and new Lightweight user accounts via .NET Identity.
        /// Both are stored in the same "external_user" DB table.
        /// </summary>
        /// <param name="originalPseudonym"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<(bool Succeeded, IEnumerable<IdentityError> Errors)> CreateNewFullAccountWithIdentityAsync(string originalPseudonym, string uppercasedPseudonym, string email, string password)
        {
            List<IdentityError> errors = new List<IdentityError>();
            if (string.IsNullOrWhiteSpace(originalPseudonym) || string.IsNullOrWhiteSpace(uppercasedPseudonym) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning($"{MethodBase.GetCurrentMethod().Name}: Missing required parameters.");
                errors.Add(new IdentityError() { Code = "Missing required parameters", Description = "Pseudonym, email, and password are required to create a new Full account." });
                return (false, errors);
            }

            var pseudonymUniquenessCheckResult = CheckPseudonymUniqueness(uppercasedPseudonym);
            if (pseudonymUniquenessCheckResult.Result.Succeeded == false)
            {
                _logger.LogWarning($"{MethodBase.GetCurrentMethod().Name}: Full account creation failed for {originalPseudonym}. Pseudonym is not unique.");
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

                //For Full accounts, we need to validate the email address.
                //.NET Identity will automatically validate the password based on rules configured in Program.cs.
                var emailValidationResult = await ValidateEmail(email);
                if (emailValidationResult.Succeeded == false)
                {
                    _logger.LogWarning($"{MethodBase.GetCurrentMethod().Name}: Full account creation failed for {originalPseudonym}. Email validation failed is not unique.");
                    return (false, emailValidationResult.Errors);
                }
                IdentityResult result = await _userManager.CreateAsync(user, password);
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

            SignInResult passwordCheckResult = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            _logger.LogInformation($"{MethodBase.GetCurrentMethod().Name}: Password check: {passwordCheckResult.Succeeded}");
            if (passwordCheckResult.Succeeded)
            {
                // Call a JS snippet that makes a fetch() request to retrieve auth cookie to log user in.
                // This is neccesary because cookies can NOT be sent to the client over Websockets.
                // so, this JS snippet makes the client send an http request, such that the response to the client will include the clients cookie.
                // of course, the API controller method hit by the fetch request still needs to validate the password because it is an open endpoint, it needs to be unprotected because it is used for login.
                // the password check done above is just a convenience to make it is easier for us to handle incorrect passwords in blazor.
                // If the fetch request returns to the client successfully, the JS snippet then reloads the page.
                // The fresh http request made by the client on page reload will now include the auth cookie.
                // consequently, this will start a Websocket session with blazor where we can use Blazor's AuthenticationStateProvider to check if the user is logged in.
                await _jSRuntime.InvokeVoidAsync("loginFullUserViaXHR", uppercasedPseudonym, password);
                return (true, null);
            }

            //SignInResult signInResult = await _signInManager.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: false);
            
            string errorMessage = passwordCheckResult.IsLockedOut ? "Account is locked."
                  : passwordCheckResult.IsNotAllowed ? "Login not allowed."
                  : passwordCheckResult.RequiresTwoFactor ? "Two-factor authentication required."
                  : "Incorrect password.";

            return (passwordCheckResult.Succeeded, errorMessage);
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
                DnsResponse dnsResponse = await ClientX.QueryDns(domain, DnsRecordType.MX, DnsEndpoint.Cloudflare, DnsSelectionStrategy.Failover, 5000);
                if (dnsResponse.Status == DnsResponseCode.NoError)
                {
                    foreach (DnsAnswer dnsAnswer in dnsResponse.Answers)
                    {
                        _logger.LogInformation($"{MethodBase.GetCurrentMethod().Name}| DNS MX query finished - RESPONSE: {dnsAnswer.Data}");
                    }
                    return true;
                }
                else
                {
                    _logger.LogInformation($"{MethodBase.GetCurrentMethod().Name}: DNS MX query failed: {dnsResponse.Status}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{MethodBase.GetCurrentMethod().Name}: Unexpected error occurred while making domain MX record check. Exception: {ex}");
                return false;
            }
        }
    }
}
