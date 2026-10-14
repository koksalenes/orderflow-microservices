using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Application.Paging;
using OrderFlow.BuildingBlocks.Web.Authentication;
using OrderFlow.BuildingBlocks.Web.Endpoints;
using OrderFlow.Payments.Application.Cards;
using OrderFlow.Payments.Application.Deposits.GetDepositSagas;
using OrderFlow.Payments.Application.Wallets.DepositFunds;
using OrderFlow.Payments.Application.Wallets.GetWallet;
using OrderFlow.Payments.Application.Wallets.GetWalletEventStream;
using OrderFlow.Payments.Application.Wallets.GetWalletTransactions;

namespace OrderFlow.Payments.Api.Wallets;

internal static class WalletEndpoints
{
    public static RouteGroupBuilder MapWalletEndpoints(this RouteGroupBuilder group)
    {
        var wallet = group.MapGroup("wallet").RequireAuthorization().WithTags("Wallet");

        wallet
            .MapGet("", GetWalletAsync)
            .WithName("GetWallet")
            .WithSummary("Returns the current balance of the signed-in user's wallet.");

        wallet
            .MapGet("transactions", GetTransactionsAsync)
            .WithName("GetWalletTransactions")
            .WithSummary("Lists the wallet's money movements, newest first.");

        wallet
            .MapGet("events", GetEventStreamAsync)
            .WithName("GetWalletEventStream")
            .WithSummary("Returns the wallet's raw event stream and its latest snapshot.");

        wallet
            .MapGet("sagas", GetDepositSagasAsync)
            .WithName("GetDepositSagas")
            .WithSummary("Returns the latest top-up sagas (settlement and compensation steps).");

        wallet
            .MapPost("deposits", DepositAsync)
            .WithName("DepositFunds")
            .WithSummary("Loads money onto the wallet from a card (test mode: no real charge).");

        return group;
    }

    private static async Task<Results<Ok<WalletDto>, ProblemHttpResult>> GetWalletAsync(
        ClaimsPrincipal user,
        IQueryHandler<GetWalletQuery, WalletDto> handler,
        CancellationToken cancellationToken
    ) =>
        (
            await handler.HandleAsync(new GetWalletQuery(user.UserId()), cancellationToken)
        ).ToHttpResult();

    private static async Task<
        Results<Ok<PagedResult<WalletTransactionDto>>, ProblemHttpResult>
    > GetTransactionsAsync(
        ClaimsPrincipal user,
        IQueryHandler<GetWalletTransactionsQuery, PagedResult<WalletTransactionDto>> handler,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PageRequest.DefaultPageSize
    ) =>
        (
            await handler.HandleAsync(
                new GetWalletTransactionsQuery(user.UserId(), page, pageSize),
                cancellationToken
            )
        ).ToHttpResult();

    private static async Task<
        Results<Ok<WalletEventStreamDto>, ProblemHttpResult>
    > GetEventStreamAsync(
        ClaimsPrincipal user,
        IQueryHandler<GetWalletEventStreamQuery, WalletEventStreamDto> handler,
        CancellationToken cancellationToken
    ) =>
        (
            await handler.HandleAsync(
                new GetWalletEventStreamQuery(user.UserId()),
                cancellationToken
            )
        ).ToHttpResult();

    private static async Task<
        Results<Ok<IReadOnlyList<DepositSagaDto>>, ProblemHttpResult>
    > GetDepositSagasAsync(
        ClaimsPrincipal user,
        IQueryHandler<GetDepositSagasQuery, IReadOnlyList<DepositSagaDto>> handler,
        CancellationToken cancellationToken,
        [FromQuery] int limit = 5
    ) =>
        (
            await handler.HandleAsync(
                new GetDepositSagasQuery(user.UserId(), limit),
                cancellationToken
            )
        ).ToHttpResult();

    private static async Task<Results<Ok<DepositResultDto>, ProblemHttpResult>> DepositAsync(
        DepositFundsRequest request,
        ClaimsPrincipal user,
        ICommandHandler<DepositFundsCommand, DepositResultDto> handler,
        CancellationToken cancellationToken
    ) =>
        (
            await handler.HandleAsync(
                new DepositFundsCommand(
                    user.UserId(),
                    request.DepositId,
                    request.Amount,
                    new CardDetails(
                        request.Card.HolderName,
                        request.Card.Number,
                        request.Card.ExpiryMonth,
                        request.Card.ExpiryYear,
                        request.Card.Cvv
                    ),
                    ReceiptRecipientOf(user, request.Locale)
                ),
                cancellationToken
            )
        ).ToHttpResult();

    private static ReceiptRecipient? ReceiptRecipientOf(ClaimsPrincipal user, string? locale) =>
        user.FindFirstValue("email") is { Length: > 0 } email
            ? new ReceiptRecipient(
                email,
                user.FindFirstValue("given_name") ?? user.FindFirstValue("name"),
                ReceiptRecipient.NormalizeLocale(locale)
            )
            : null;
}
