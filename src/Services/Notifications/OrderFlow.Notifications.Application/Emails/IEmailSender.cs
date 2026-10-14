namespace OrderFlow.Notifications.Application.Emails;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
