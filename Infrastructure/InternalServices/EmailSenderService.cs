using System.Net;
using System.Net.Mail;
using Application.IServices.IInternal;
using Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.InternalServices;

public class EmailSenderService : IEmailSenderService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailSenderService> _logger;
    public EmailSenderService(IOptions<EmailSettings> settings, ILogger<EmailSenderService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            using var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.FromAddress, _settings.Password),
                EnableSsl = true
            };

            using var mailMessage = new MailMessage(_settings.FromAddress, email, subject, htmlMessage)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", email);
            throw;
        }
    }
}
