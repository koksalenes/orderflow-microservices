using System.Data.Common;
using Dapper;
using OrderFlow.Payments.Domain.EventSourcing;
using OrderFlow.Payments.Domain.Wallets;

namespace OrderFlow.Payments.Infrastructure.Wallets;

internal static class WalletProjector
{
    private const string OpenSql = """
        INSERT INTO wallet_balances (owner_id, balance, currency, version, updated_at)
        VALUES (@OwnerId, 0, @Currency, @Version, @OccurredAt);
        """;

    private const string ChangeBalanceSql = """
        UPDATE wallet_balances
        SET balance = balance + @Delta, version = @Version, updated_at = @OccurredAt
        WHERE owner_id = @OwnerId
        RETURNING balance;
        """;

    private const string DepositSql = """
        INSERT INTO wallet_transactions
            (id, owner_id, type, amount, balance_after, currency, card_brand, card_last4, status, occurred_at)
        VALUES
            (@Id, @OwnerId, 'deposit', @Amount, @BalanceAfter, @Currency, @CardBrand, @CardLast4, 'pending', @OccurredAt);
        """;

    private const string SettledSql = """
        UPDATE wallet_transactions SET status = 'settled' WHERE id = @DepositId;
        """;

    private const string ReversalSql = """
        UPDATE wallet_transactions SET status = 'reversed' WHERE id = @DepositId;

        INSERT INTO wallet_transactions
            (id, owner_id, type, amount, balance_after, currency, card_brand, card_last4,
             reference_id, reason, occurred_at)
        SELECT @ReversalId, @OwnerId, 'reversal', -@Amount, @BalanceAfter, @Currency,
               deposit.card_brand, deposit.card_last4, @DepositId, @ReasonCode, @OccurredAt
        FROM (SELECT 1) AS one
        LEFT JOIN wallet_transactions deposit ON deposit.id = @DepositId;
        """;

    private const string VersionSql = """
        UPDATE wallet_balances SET version = @Version, updated_at = @OccurredAt WHERE owner_id = @OwnerId;
        """;

    public static async Task ProjectAsync(
        DbConnection connection,
        DbTransaction transaction,
        string ownerId,
        IDomainEvent domainEvent,
        int version,
        CancellationToken cancellationToken
    )
    {
        Task<int> Execute(string sql, object parameters)
        {
            return connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    parameters,
                    transaction,
                    cancellationToken: cancellationToken
                )
            );
        }

        Task<decimal> ChangeBalance(decimal delta, DateTimeOffset occurredAt)
        {
            return connection.ExecuteScalarAsync<decimal>(
                new CommandDefinition(
                    ChangeBalanceSql,
                    new
                    {
                        OwnerId = ownerId,
                        Delta = delta,
                        Version = version,
                        OccurredAt = occurredAt,
                    },
                    transaction,
                    cancellationToken: cancellationToken
                )
            );
        }

        switch (domainEvent)
        {
            case WalletOpened opened:
                await Execute(
                        OpenSql,
                        new
                        {
                            OwnerId = ownerId,
                            opened.Currency,
                            Version = version,
                            opened.OccurredAt,
                        }
                    )
                    .ConfigureAwait(false);
                break;

            case FundsDeposited deposited:
                var balanceAfterDeposit = await ChangeBalance(
                        deposited.Amount,
                        deposited.OccurredAt
                    )
                    .ConfigureAwait(false);
                await Execute(
                        DepositSql,
                        new
                        {
                            Id = deposited.DepositId,
                            OwnerId = ownerId,
                            deposited.Amount,
                            BalanceAfter = balanceAfterDeposit,
                            deposited.Currency,
                            deposited.CardBrand,
                            deposited.CardLast4,
                            deposited.OccurredAt,
                        }
                    )
                    .ConfigureAwait(false);
                break;

            case DepositSettled settled:
                await Execute(SettledSql, new { settled.DepositId }).ConfigureAwait(false);
                await Execute(
                        VersionSql,
                        new
                        {
                            OwnerId = ownerId,
                            Version = version,
                            settled.OccurredAt,
                        }
                    )
                    .ConfigureAwait(false);
                break;

            case DepositReversed reversed:
                var balanceAfterReversal = await ChangeBalance(
                        -reversed.Amount,
                        reversed.OccurredAt
                    )
                    .ConfigureAwait(false);
                await Execute(
                        ReversalSql,
                        new
                        {
                            reversed.DepositId,
                            reversed.ReversalId,
                            OwnerId = ownerId,
                            reversed.Amount,
                            BalanceAfter = balanceAfterReversal,
                            reversed.Currency,
                            reversed.ReasonCode,
                            reversed.OccurredAt,
                        }
                    )
                    .ConfigureAwait(false);
                break;

            default:
                throw new InvalidOperationException(
                    $"WalletProjector cannot project '{domainEvent.GetType().Name}'."
                );
        }
    }
}
