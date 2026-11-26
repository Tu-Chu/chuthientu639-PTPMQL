using System.Net;
using System.Net.Mail;

namespace DemoMVC.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            var host = _configuration["Email:SmtpHost"];
            var port = int.TryParse(_configuration["Email:SmtpPort"], out var configuredPort)
                ? configuredPort
                : 587;
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];
            var from = _configuration["Email:From"] ?? username;

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
            {
                _logger.LogWarning("Email SMTP chưa được cấu hình. Email giả lập tới {Email}: {Subject}", toEmail, subject);
                _logger.LogInformation("Nội dung email (dev): {Body}", htmlBody);
                return;
            }

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = bool.TryParse(_configuration["Email:EnableSsl"], out var ssl) && ssl,
                Credentials = string.IsNullOrWhiteSpace(username)
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(username, password)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(from),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);
            await client.SendMailAsync(message);
        }
    }
}
