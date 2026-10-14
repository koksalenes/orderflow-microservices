using Dapper;
using Npgsql;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Application.Paging;
using OrderFlow.BuildingBlocks.Domain;
using OrderFlow.Payments.Application.Wallets.GetWalletTransactions;

namespace OrderFlow.Payments.Infrastructure.Wallets.GetWalletTransactions;

internal sealed class GetWalletTransactionsQueryHandler(NpgsqlDataSource dataSource)
    : IQueryHandler<GetWalletTransactionsQuery, PagedResult<WalletTransactionDto>>
{
    private const string Sql = """
        SELECT id AS Id, type AS Type, amount AS Amount, balance_after AS BalanceAfter,
               currency AS Currency, card_brand AS CardBrand, card_last4 AS CardLast4,
               status AS Status, reference_id AS ReferenceId, reason AS Reason,
               occurred_at AS OccurredAt
        FROM wallet_transactions
        WHERE owner_id = @OwnerId
        ORDER BY occurred_at DESC, id DESC
        LIMIT @PageSize OFFSET @Offset;

        SELECT count(*)::int FROM wallet_transactions WHERE owner_id = @OwnerId;
        """;

    public async Task<Result<PagedResult<WalletTransactionDto>>> HandleAsync(
        GetWalletTransactionsQuery query,
        CancellationToken cancellationToken
    )
    {
        var connection = await dataSource
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var reader = await connection
                .QueryMultipleAsync(
                    new CommandDefinition(
                        Sql,
                        new
                        {
                            query.OwnerId,
                            query.PageSize,
                            Offset = PageRequest.Offset(query.Page, query.PageSize),
                        },
                        cancellationToken: cancellationToken
                    )
                )
                .ConfigureAwait(false);
            await using (reader.ConfigureAwait(false))
            {
                var items = await reader.ReadAsync<TransactionRow>().ConfigureAwait(false);
                var totalCount = await reader.ReadSingleAsync<int>().ConfigureAwait(false);

                return Result.Success(
                    new PagedResult<WalletTransactionDto>(
                        [.. items.Select(row => row.ToDto())],
                        query.Page,
                        query.PageSize,
                        totalCount
                    )
                );
            }
        }
    }

    internal sealed class TransactionRow
    {
        public Guid Id { get; init; }

        public string Type { get; init; } = string.Empty;

        public decimal Amount { get; init; }

        public decimal BalanceAfter { get; init; }

        public string Currency { get; init; } = string.Empty;

        public string? CardBrand { get; init; }

        public string? CardLast4 { get; init; }

        public string? Status { get; init; }

        public Guid? ReferenceId { get; init; }

        public string? Reason { get; init; }

        public DateTimeOffset OccurredAt { get; init; }

        public WalletTransactionDto ToDto() =>
            new(
                Id,
                Type,
                Amount,
                BalanceAfter,
                Currency,
                CardBrand,
                CardLast4,
                Status,
                ReferenceId,
                Reason,
                OccurredAt
            );
    }
}
