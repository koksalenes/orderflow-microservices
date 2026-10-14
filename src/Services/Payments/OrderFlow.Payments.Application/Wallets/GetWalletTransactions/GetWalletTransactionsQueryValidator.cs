using FluentValidation;
using OrderFlow.BuildingBlocks.Application.Paging;

namespace OrderFlow.Payments.Application.Wallets.GetWalletTransactions;

public sealed class GetWalletTransactionsQueryValidator
    : AbstractValidator<GetWalletTransactionsQuery>
{
    public GetWalletTransactionsQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1).WithErrorCode("validation.min");
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, PageRequest.MaxPageSize)
            .WithErrorCode("validation.range");
    }
}
