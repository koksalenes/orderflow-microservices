using FluentValidation;
using OrderFlow.BuildingBlocks.Application.Paging;

namespace OrderFlow.Orders.Application.Orders.GetMyOrders;

public sealed class GetMyOrdersQueryValidator : AbstractValidator<GetMyOrdersQuery>
{
    public GetMyOrdersQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1).WithErrorCode("validation.min");
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, PageRequest.MaxPageSize)
            .WithErrorCode("validation.range");
    }
}
