using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Web.Authentication;
using OrderFlow.BuildingBlocks.Web.Endpoints;
using OrderFlow.Payments.Api.Wallets;
using OrderFlow.Payments.Application.CardPayments.AuthorizeCardPayment;
using OrderFlow.Payments.Application.Cards;

namespace OrderFlow.Payments.Api.CardPayments;

internal static class CardPaymentEndpoints
{
    public static RouteGroupBuilder MapCardPaymentEndpoints(this RouteGroupBuilder group)
    {
        group
            .MapPost("card-payments", AuthorizeAsync)
            .RequireAuthorization()
            .WithTags("Card payments")
            .WithName("AuthorizeCardPayment")
            .WithSummary(
                "Authorizes a card for an order total; the order captures it later (test mode)."
            );

        return group;
    }

    private static async Task<Results<Ok<CardPaymentDto>, ProblemHttpResult>> AuthorizeAsync(
        AuthorizeCardPaymentRequest request,
        ClaimsPrincipal user,
        ICommandHandler<AuthorizeCardPaymentCommand, CardPaymentDto> handler,
        CancellationToken cancellationToken
    ) =>
        (
            await handler.HandleAsync(
                new AuthorizeCardPaymentCommand(
                    user.UserId(),
                    request.Amount,
                    new CardDetails(
                        request.Card.HolderName,
                        request.Card.Number,
                        request.Card.ExpiryMonth,
                        request.Card.ExpiryYear,
                        request.Card.Cvv
                    )
                ),
                cancellationToken
            )
        ).ToHttpResult();
}

internal sealed record AuthorizeCardPaymentRequest(decimal Amount, CardRequest Card);
