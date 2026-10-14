using Dapper;
using Npgsql;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Domain;
using OrderFlow.Payments.Application.Wallets.GetWallet;
using OrderFlow.Payments.Domain.Wallets;

namespace OrderFlow.Payments.Infrastructure.Wallets.GetWallet;

internal sealed class GetWalletQueryHandler(NpgsqlDataSource dataSource)
    : IQueryHandler<GetWalletQuery, WalletDto>
{
    private const string Sql = """
        SELECT balance AS Balance, currency AS Currency, version AS Version, updated_at AS UpdatedAt
        FROM wallet_balances
        WHERE owner_id = @OwnerId;
        """;

    public async Task<Result<WalletDto>> HandleAsync(
        GetWalletQuery query,
        CancellationToken cancellationToken
    )
    {
        var connection = await dataSource
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var wallet = await connection
                .QuerySingleOrDefaultAsync<WalletRow>(
                    new CommandDefinition(
                        Sql,
                        new { query.OwnerId },
                        cancellationToken: cancellationToken
                    )
                )
                .ConfigureAwait(false);

            return Result.Success(
                wallet?.ToDto() ?? new WalletDto(0, Wallet.DefaultCurrency, 0, null)
            );
        }
    }

    internal sealed class WalletRow
    {
        public decimal Balance { get; init; }

        public string Currency { get; init; } = string.Empty;

        public int Version { get; init; }

        public DateTimeOffset UpdatedAt { get; init; }

        public WalletDto ToDto() => new(Balance, Currency, Version, UpdatedAt);
    }
}
