using Microsoft.Extensions.Logging;

namespace Comax.VipWorker.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // TODO: Tích hợp SMTP thực tế ở đây (dùng MailKit hoặc SmtpClient)
            // Hiện tại chỉ Log ra console để giả lập
            _logger.LogInformation($"[EMAIL SENT] To: {toEmail} | Subject: {subject} | Content: {body}");
            await Task.CompletedTask;
        }
    }
}