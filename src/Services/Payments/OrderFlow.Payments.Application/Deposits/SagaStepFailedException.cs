using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.Payments.Application.Deposits;

public sealed class SagaStepFailedException : Exception
{
    public SagaStepFailedException() { }

    public SagaStepFailedException(string message)
        : base(message) { }

    public SagaStepFailedException(string message, Exception innerException)
        : base(message, innerException) { }

    public SagaStepFailedException(Guid depositId, string step, Error error)
        : base($"Deposit saga '{depositId}' step '{step}' failed: {error?.Code} {error?.Message}")
    { }
}
