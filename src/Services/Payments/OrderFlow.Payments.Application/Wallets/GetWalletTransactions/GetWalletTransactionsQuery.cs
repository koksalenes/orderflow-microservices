using OrderFlow.BuildingBlocks.Application.Paging;

namespace OrderFlow.Payments.Application.Wallets.GetWalletTransactions;

public sealed record GetWalletTransactionsQuery(
    string OwnerId,
    int Page = 1,
    int PageSize = PageRequest.DefaultPageSize
);
