using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.Payments.Domain.Deposits;

public sealed class DepositSaga : AggregateRoot<Guid>
{
    public const string StepStarted = "started";
    public const string StepSettlementRequested = "settlement_requested";
    public const string StepSettled = "settled";
    public const string StepSettlementFailed = "settlement_failed";
    public const string StepWalletReversed = "wallet_reversed";
    public const string StepAuthorizationVoided = "authorization_voided";
    public const string StepCompensated = "compensated";

    private readonly List<DepositSagaStep> _steps = [];

    private DepositSaga() { }

    public string OwnerId { get; private set; } = string.Empty;

    public decimal Amount { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public string CardBrand { get; private set; } = string.Empty;

    public string CardLast4 { get; private set; } = string.Empty;

    public string AuthorizationCode { get; private set; } = string.Empty;

    public string? RecipientEmail { get; private set; }

    public string? RecipientName { get; private set; }

    public string Locale { get; private set; } = string.Empty;

    public DepositSagaState State { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTimeOffset StartedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyList<DepositSagaStep> Steps => _steps;

    public uint Version { get; }

    public bool IsFinished => State is DepositSagaState.Completed or DepositSagaState.Compensated;

    public static DepositSaga Start(
        Guid depositId,
        string ownerId,
        decimal amount,
        string currency,
        string cardBrand,
        string cardLast4,
        string authorizationCode,
        string? recipientEmail,
        string? recipientName,
        string locale,
        DateTimeOffset now
    )
    {
        var saga = new DepositSaga
        {
            Id = depositId,
            OwnerId = ownerId,
            Amount = amount,
            Currency = currency,
            CardBrand = cardBrand,
            CardLast4 = cardLast4,
            AuthorizationCode = authorizationCode,
            RecipientEmail = recipientEmail,
            RecipientName = recipientName,
            Locale = locale,
            State = DepositSagaState.AwaitingSettlement,
            StartedAt = now,
            UpdatedAt = now,
        };
        saga.Record(StepStarted, now);
        saga.Record(StepSettlementRequested, now, authorizationCode);
        return saga;
    }

    public Result MarkSettled(DateTimeOffset now)
    {
        if (State != DepositSagaState.AwaitingSettlement)
        {
            return Result.Failure(DepositSagaErrors.InvalidTransition(Id, State, StepSettled));
        }

        State = DepositSagaState.Completed;
        Record(StepSettled, now);
        return Result.Success();
    }

    public Result BeginCompensation(string reason, DateTimeOffset now)
    {
        if (State != DepositSagaState.AwaitingSettlement)
        {
            return Result.Failure(
                DepositSagaErrors.InvalidTransition(Id, State, StepSettlementFailed)
            );
        }

        State = DepositSagaState.Compensating;
        FailureReason = reason;
        Record(StepSettlementFailed, now, reason);
        return Result.Success();
    }

    public void RecordCompensationStep(string step, DateTimeOffset now, string? detail = null)
    {
        if (_steps.TrueForAll(existing => existing.Name != step))
        {
            Record(step, now, detail);
        }
    }

    public Result MarkCompensated(DateTimeOffset now)
    {
        if (State != DepositSagaState.Compensating)
        {
            return Result.Failure(DepositSagaErrors.InvalidTransition(Id, State, StepCompensated));
        }

        State = DepositSagaState.Compensated;
        Record(StepCompensated, now);
        return Result.Success();
    }

    private void Record(string step, DateTimeOffset now, string? detail = null)
    {
        _steps.Add(new DepositSagaStep(step, now, detail));
        UpdatedAt = now;
    }
}
