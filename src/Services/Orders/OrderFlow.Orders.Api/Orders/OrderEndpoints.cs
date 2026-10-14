using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Application.Paging;
using OrderFlow.BuildingBlocks.Web.Authentication;
using OrderFlow.BuildingBlocks.Web.Endpoints;
using OrderFlow.Orders.Application.Orders.GetMyOrders;
using OrderFlow.Orders.Application.Orders.GetOrder;
using OrderFlow.Orders.Application.Orders.PlaceOrder;

namespace OrderFlow.Orders.Api.Orders;

internal static class OrderEndpoints
{
    private static readonly HashSet<string> Locales = new(StringComparer.Ordinal) { "en", "tr" };

    public static RouteGroupBuilder MapOrderEndpoints(this RouteGroupBuilder group)
    {
        group.RequireAuthorization().WithTags("Orders");

        group
            .MapPost("", PlaceOrderAsync)
            .WithName("PlaceOrder")
            .WithSummary(
                "Places an order; stock, payment, and fulfillment continue asynchronously (order saga)."
            );

        group
            .MapGet("", GetMyOrdersAsync)
            .WithName("GetMyOrders")
            .WithSummary("Lists the signed-in user's orders, newest first.");

        group
            .MapGet("{id:guid}", GetOrderAsync)
            .WithName("GetOrder")
            .WithSummary("Returns one of the signed-in user's orders with its saga steps.");

        return group;
    }

    private static async Task<Results<Created<OrderDto>, ProblemHttpResult>> PlaceOrderAsync(
        PlaceOrderRequest request,
        ClaimsPrincipal user,
        ICommandHandler<PlaceOrderCommand, OrderDto> handler,
        CancellationToken cancellationToken
    ) =>
        (
            await handler.HandleAsync(
                new PlaceOrderCommand(
                    user.UserId(),
                    user.FindFirstValue("email"),
                    user.FindFirstValue("given_name") ?? user.FindFirstValue("name"),
                    request.Locale is not null && Locales.Contains(request.Locale)
                        ? request.Locale
                        : "en",
                    request.Lines ?? [],
                    request.ShippingAddress!,
                    request.PaymentMethod ?? string.Empty,
                    request.CardPaymentId,
                    request.CardBrand,
                    request.CardLast4,
                    request.Scenario
                ),
                cancellationToken
            )
        ).ToCreatedHttpResult(order => $"/api/v1/orders/{order.Id}");

    private static async Task<
        Results<Ok<PagedResult<OrderSummaryDto>>, ProblemHttpResult>
    > GetMyOrdersAsync(
        ClaimsPrincipal user,
        IQueryHandler<GetMyOrdersQuery, PagedResult<OrderSummaryDto>> handler,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PageRequest.DefaultPageSize
    ) =>
        (
            await handler.HandleAsync(
                new GetMyOrdersQuery(user.UserId(), page, pageSize),
                cancellationToken
            )
        ).ToHttpResult();

    private static async Task<Results<Ok<OrderDto>, ProblemHttpResult>> GetOrderAsync(
        Guid id,
        ClaimsPrincipal user,
        IQueryHandler<GetOrderQuery, OrderDto> handler,
        CancellationToken cancellationToken
    ) =>
        (
            await handler.HandleAsync(new GetOrderQuery(user.UserId(), id), cancellationToken)
        ).ToHttpResult();
}

internal sealed record PlaceOrderRequest(
    IReadOnlyList<PlaceOrderLine>? Lines,
    PlaceOrderAddress? ShippingAddress,
    string? PaymentMethod,
    Guid? CardPaymentId,
    string? CardBrand,
    string? CardLast4,
    string? Scenario,
    string? Locale
);
