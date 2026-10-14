using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.Payments.Domain.Deposits;

public static class DepositSagaErrors
{
    public static Error InvalidTransition(Guid depositId, DepositSagaState from, string step) =>
        Error.Conflict(
            "payments.saga_invalid_transition",
            $"Deposit saga '{depositId}' cannot run '{step}' in state {from}."
        );
}
