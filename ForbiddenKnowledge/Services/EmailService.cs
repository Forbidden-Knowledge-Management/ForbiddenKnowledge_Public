using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ForbiddenKnowledge.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Forbidden Knowledge Administrator", "admin@forbidden-knowledge.com"));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            string password = _configuration.GetValue<string>("admin@forbidden-knowledge.com_email_password");

            using SmtpClient client = new SmtpClient();
            try
            {
                await client.ConnectAsync("mail.morelliwebservices.com", 465, SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync("admin@forbidden-knowledge.com", password);
                string emailServerResponse = await client.SendAsync(message);

                _logger.LogInformation($"Password reset email sent to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email: {ex.Message}");
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }

        }




    }
}
