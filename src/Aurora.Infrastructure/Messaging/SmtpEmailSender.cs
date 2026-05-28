using System.Net;
using System.Net.Mail;
using Aurora.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aurora.Infrastructure.Messaging;

public class SmtpEmailSender(IOptions<SmtpSettings> settings, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        var smtp = settings.Value;
        if (!smtp.Enabled)
        {
            logger.LogInformation("Email queued to {Email} with subject {Subject}. Body: {Body}", to, subject, body);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(smtp.FromEmail, smtp.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(to);

        using var client = new SmtpClient(smtp.Host, smtp.Port)
        {
            EnableSsl = smtp.EnableSsl,
            Credentials = string.IsNullOrWhiteSpace(smtp.Username)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(smtp.Username, smtp.Password)
        };

        await client.SendMailAsync(message, ct);
    }
}
