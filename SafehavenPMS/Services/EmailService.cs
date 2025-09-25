using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SafehavenPMS.Services
{
    public interface IEmailService
    {
        Task SendStaffCredentialsAsync(string toEmail, string username, string password, string? staffName = null);
    }

    public class EmailService : IEmailService
    {
        private readonly SmtpOptions _options;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _logger = logger;
            _options = configuration.GetSection("Smtp").Get<SmtpOptions>() ?? throw new InvalidOperationException("SMTP configuration missing");
        }

        /// <summary>
        /// Sends staff username and password to the specified email address.
        /// NOTE: Sending plaintext passwords by email is insecure. Prefer issuing a temporary one-time token/password and forcing a reset.
        /// </summary>
        public async Task SendStaffCredentialsAsync(string toEmail, string username, string password, string? staffName = null)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) throw new ArgumentException("toEmail required", nameof(toEmail));
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("username required", nameof(username));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("password required", nameof(password));

            var fromAddress = new MailAddress(_options.FromAddress, _options.FromDisplayName);
            var toAddress = new MailAddress(toEmail);

            var greeting = string.IsNullOrWhiteSpace(staffName)
                ? "Hello,"
                : $"Hello {WebUtility.HtmlEncode(staffName)},";

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = "Your Safehaven Recovery Village Staff Account",
                IsBodyHtml = true,
                Body =
                    $@"<p>{greeting}</p>
                       <p>Your account has been created. Below are your credentials:</p>
                       <ul>
                         <li><strong>Username:</strong> {WebUtility.HtmlEncode(username)}</li>
                         <li><strong>Password:</strong> {WebUtility.HtmlEncode(password)}</li>
                       </ul>
                       <p>You can change your password anytime from your account settings.</p>
                       <p>Regards,<br/>Safehaven Recovery Village</p>"
            };

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.UseSsl,
                Credentials = new NetworkCredential(_options.UserName, _options.Password),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = _options.TimeoutMs
            };

            try
            {
                await client.SendMailAsync(message).ConfigureAwait(false);
                _logger.LogInformation("Sent credentials email to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send credentials email to {Email}", toEmail);
                throw;
            }
        }
    }

    public class SmtpOptions
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 25;
        public bool UseSsl { get; set; } = false;
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public string FromAddress { get; set; } = "no-reply@safehavenpms.local";
        public string FromDisplayName { get; set; } = "SafehavenPMS";
        public int TimeoutMs { get; set; } = 100000;
    }
}