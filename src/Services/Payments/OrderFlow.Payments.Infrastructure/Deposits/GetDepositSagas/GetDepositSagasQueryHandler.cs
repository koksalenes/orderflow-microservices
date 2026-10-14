using System.Text.Json;
using Dapper;
using Npgsql;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Domain;
using OrderFlow.Payments.Application.Deposits.GetDepositSagas;

namespace OrderFlow.Payments.Infrastructure.Deposits.GetDepositSagas;

internal sealed class GetDepositSagasQueryHandler(NpgsqlDataSource dataSource)
    : IQueryHandler<GetDepositSagasQuery, IReadOnlyList<DepositSagaDto>>
{
    private const string Sql = """
        SELECT deposit_id AS DepositId, state AS State, amount AS Amount, currency AS Currency,
               card_brand AS CardBrand, card_last4 AS CardLast4, failure_reason AS FailureReason,
               started_at AS StartedAt, updated_at AS UpdatedAt, steps::text AS Steps
        FROM deposit_sagas
        WHERE owner_id = @OwnerId
        ORDER BY started_at DESC
        LIMIT @Limit;
        """;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<IReadOnlyList<DepositSagaDto>>> HandleAsync(
        GetDepositSagasQuery query,
        CancellationToken cancellationToken
    )
    {
        var connection = await dataSource
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var rows = await connection
                .QueryAsync<SagaRow>(
                    new CommandDefinition(
                        Sql,
                        new { query.OwnerId, query.Limit },
                        cancellationToken: cancellationToken
                    )
                )
                .ConfigureAwait(false);

            return Result.Success<IReadOnlyList<DepositSagaDto>>([
                .. rows.Select(row => row.ToDto()),
            ]);
        }
    }

    internal sealed class SagaRow
    {
        public Guid DepositId { get; init; }

        public string State { get; init; } = string.Empty;

        public decimal Amount { get; init; }

        public string Currency { get; init; } = string.Empty;

        public string CardBrand { get; init; } = string.Empty;

        public string CardLast4 { get; init; } = string.Empty;

        public string? FailureReason { get; init; }

        public DateTimeOffset StartedAt { get; init; }

        public DateTimeOffset UpdatedAt { get; init; }

        public string? Steps { get; init; }

        public DepositSagaDto ToDto() =>
            new(
                DepositId,
                State,
                Amount,
                Currency,
                CardBrand,
                CardLast4,
                FailureReason,
                StartedAt,
                UpdatedAt,
                Steps is null
                    ? []
                    : JsonSerializer.Deserialize<List<DepositSagaStepDto>>(Steps, JsonOptions) ?? []
            );
    }
}
