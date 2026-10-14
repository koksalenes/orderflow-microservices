using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.BuildingBlocks.Application.Validation;

public sealed record ValidationError(IReadOnlyDictionary<string, string[]> Errors)
    : Error(ErrorCode, "One or more validation errors occurred.", ErrorType.Validation)
{
    public const string ErrorCode = "common.validation_failed";
}
