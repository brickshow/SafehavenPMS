using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.Services
{
    public interface IEmailService
    {
        Task SendStaffCredentialsAsync(string toEmail, string username, string password, string? staffName = null);
        Task SendOtpAsync(string displayName, string toEmail, string otpCode);
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

        /// <summary>
        /// Sends a 6-digit OTP code to the specified email address.
        /// </summary>
        public async Task SendOtpAsync(string displayName, string toEmail, string otpCode)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) throw new ArgumentException("toEmail required", nameof(toEmail));
            if (string.IsNullOrWhiteSpace(otpCode)) throw new ArgumentException("otpCode required", nameof(otpCode));

            var fromAddress = new MailAddress(_options.FromAddress, _options.FromDisplayName);
            var toAddress = new MailAddress(toEmail);

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = "Your Safehaven Password Reset Code",
                IsBodyHtml = true,
                Body = $@"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                          <div style='text-align: center; margin-bottom: 30px;'>
                            <h2 style='color: #004D4D; margin: 0;'>Safehaven Recovery Village</h2>
                            <p style='color: #666; margin: 5px 0 0 0;'>Password Reset Request</p>
                          </div>
                          
                          <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                            <p style='margin: 0 0 15px 0; font-size: 16px;'>Hello {WebUtility.HtmlEncode(displayName)},</p>
                            <p style='margin: 0 0 20px 0; color: #333;'>You have requested to reset your password. Please use the following verification code:</p>
                            
                            <div style='text-align: center; margin: 25px 0;'>
                              <div style='display: inline-block; background-color: #004D4D; color: white; padding: 15px 25px; border-radius: 6px; font-size: 24px; font-weight: bold; letter-spacing: 4px;'>
                                {WebUtility.HtmlEncode(otpCode)}
                              </div>
                            </div>
                            
                            <p style='margin: 20px 0 0 0; color: #666; font-size: 14px;'>
                              <strong>Important:</strong> This code will expire in 10 minutes. If you did not request a password reset, please ignore this email and contact support if you have concerns about your account security.
                            </p>
                          </div>
                          
                          <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee;'>
                            <p style='margin: 0; color: #666; font-size: 12px;'>
                              This is an automated message from Safehaven Recovery Village.<br/>
                              Please do not reply to this email.
                            </p>
                          </div>
                        </div>"
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
                _logger.LogInformation("Sent OTP email to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email}", toEmail);
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
