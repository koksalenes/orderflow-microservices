using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using OrderFlow.Notifications.Application.Emails;

namespace OrderFlow.Notifications.Infrastructure.Email;

internal sealed partial class SmtpEmailSender(
    IOptions<SmtpOptions> options,
    ILogger<SmtpEmailSender> logger
) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        var settings = options.Value;
        using var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
        mime.To.Add(new MailboxAddress(message.ToName ?? string.Empty, message.To));
        mime.Subject = message.Subject;
        if (message.MessageId is not null)
        {
            mime.MessageId = message.MessageId;
        }

        mime.Body = new BodyBuilder
        {
            HtmlBody = message.HtmlBody,
            TextBody = message.TextBody,
        }.ToMessageBody();

        using var client = new SmtpClient();
        await client
            .ConnectAsync(
                settings.Host,
                settings.Port,
                settings.UseTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                cancellationToken
            )
            .ConfigureAwait(false);
        if (!string.IsNullOrEmpty(settings.Username))
        {
            await client
                .AuthenticateAsync(
                    settings.Username,
                    settings.Password ?? string.Empty,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        await client.SendAsync(mime, cancellationToken).ConfigureAwait(false);
        await client.DisconnectAsync(quit: true, cancellationToken).ConfigureAwait(false);

        LogSent(message.Subject, settings.Host);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Email \"{Subject}\" sent through {SmtpHost}"
    )]
    private partial void LogSent(string subject, string smtpHost);
}
