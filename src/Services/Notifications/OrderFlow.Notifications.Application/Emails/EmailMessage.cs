namespace OrderFlow.Notifications.Application.Emails;

public sealed record EmailMessage(
    string To,
    string? ToName,
    string Subject,
    string HtmlBody,
    string TextBody,
    string? MessageId = null
);
