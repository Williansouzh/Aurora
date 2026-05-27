using Aurora.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;

namespace Aurora.Infrastructure.Messaging;

public class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        logger.LogInformation("Email queued to {Email} with subject {Subject}. Body: {Body}", to, subject, body);
        return Task.CompletedTask;
    }
}
