using System.Security.Cryptography;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Domain;
using OrderFlow.Payments.Application.Cards;
using OrderFlow.Payments.Contracts.Wallets;
using OrderFlow.Payments.Domain.Wallets;

namespace OrderFlow.Payments.Application.Wallets.DepositFunds;

internal sealed class DepositFundsCommandHandler(
    IWalletRepository wallets,
    IPaymentGateway paymentGateway,
    IOutbox outbox,
    TimeProvider timeProvider
) : ICommandHandler<DepositFundsCommand, DepositResultDto>
{
    private const int MaxAttempts = 6;

    public async Task<Result<DepositResultDto>> HandleAsync(
        DepositFundsCommand command,
        CancellationToken cancellationToken
    )
    {
        CardAuthorization? authorization = null;
        var saved = Result.Success();

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var now = timeProvider.GetUtcNow();
            var wallet =
                await wallets.GetAsync(command.OwnerId, cancellationToken).ConfigureAwait(false)
                ?? Wallet.Open(command.OwnerId, Wallet.DefaultCurrency, now);

            var check = wallet.CanDeposit(command.Amount);
            if (check.IsFailure)
            {
                return Result.Failure<DepositResultDto>(check.Error!);
            }

            if (authorization is null)
            {
                var charge = await paymentGateway
                    .ChargeAsync(
                        new CardCharge(
                            command.DepositId,
                            command.Amount,
                            wallet.Currency,
                            command.Card
                        ),
                        cancellationToken
                    )
                    .ConfigureAwait(false);
                if (charge.IsFailure)
                {
                    return Result.Failure<DepositResultDto>(charge.Error!);
                }

                authorization = charge.Value;
            }

            var deposit = wallet.Deposit(
                command.DepositId,
                command.Amount,
                command.Card.Brand,
                command.Card.Last4,
                authorization.AuthorizationCode,
                now
            );
            if (deposit.IsFailure)
            {
                return Result.Failure<DepositResultDto>(deposit.Error!);
            }

            outbox.Enqueue(
                WalletFundsDeposited.MessageType,
                new WalletFundsDeposited(
                    command.DepositId,
                    command.OwnerId,
                    command.Recipient?.Email,
                    command.Recipient?.Name,
                    command.Recipient?.Locale ?? ReceiptRecipient.DefaultLocale,
                    command.Amount,
                    wallet.Currency,
                    wallet.Balance,
                    command.Card.Brand,
                    command.Card.Last4,
                    authorization.AuthorizationCode,
                    now
                )
            );

            saved = await wallets.SaveAsync(wallet, cancellationToken).ConfigureAwait(false);
            if (saved.IsSuccess)
            {
                return Result.Success(
                    new DepositResultDto(
                        command.DepositId,
                        command.Amount,
                        wallet.Currency,
                        wallet.Balance,
                        command.Card.Brand,
                        command.Card.Last4,
                        authorization.AuthorizationCode,
                        wallet.Version
                    )
                );
            }

            if (saved.Error!.Code != WalletErrors.ConcurrencyConflict(wallet.StreamId).Code)
            {
                break;
            }

            var delay = TimeSpan.FromMilliseconds(RandomNumberGenerator.GetInt32(10, 50) * attempt);
            await Task.Delay(delay, timeProvider, cancellationToken).ConfigureAwait(false);
        }

        return Result.Failure<DepositResultDto>(saved.Error!);
    }
}
