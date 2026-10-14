using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Notifications.Application.Emails;
using OrderFlow.Orders.Contracts.Orders;

namespace OrderFlow.Notifications.Application.Orders;

internal sealed class OrderStatusChangedHandler(
    IEmailSender emailSender,
    StorefrontLinks links,
    TimeProvider timeProvider
) : IIntegrationMessageHandler<OrderStatusChanged>
{
    public Task HandleAsync(
        OrderStatusChanged message,
        MessageContext context,
        CancellationToken cancellationToken
    ) =>
        OrderEmail.Create(message, links, timeProvider.GetUtcNow().Year) is { } email
            ? emailSender.SendAsync(email, cancellationToken)
            : Task.CompletedTask;
}
