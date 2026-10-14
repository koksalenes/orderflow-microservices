using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Notifications.Application.Emails;
using OrderFlow.Payments.Contracts.Wallets;

namespace OrderFlow.Notifications.Application.Deposits;

internal sealed class WalletFundsDepositedHandler(
    IEmailSender emailSender,
    StorefrontLinks links,
    TimeProvider timeProvider
) : IIntegrationMessageHandler<WalletFundsDeposited>
{
    public Task HandleAsync(
        WalletFundsDeposited message,
        MessageContext context,
        CancellationToken cancellationToken
    ) =>
        WalletEmailModel.From(message) is { } model
            ? emailSender.SendAsync(
                WalletEmail.Create(model, links, timeProvider.GetUtcNow().Year),
                cancellationToken
            )
            : Task.CompletedTask;
}
