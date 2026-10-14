import { queryOptions } from "@tanstack/react-query";
import type { Page } from "@/features/catalog/catalog-api";
import { http } from "@/lib/http";

export type Wallet = { balance: number; currency: string; version: number; updatedAt: string | null };

export type WalletTransaction = {
    id: string;
    type: "deposit" | "reversal" | "payment" | "refund";
    amount: number;
    balanceAfter: number;
    currency: string;
    cardBrand: string | null;
    cardLast4: string | null;
    status: "pending" | "settled" | "reversed" | null;
    referenceId: string | null;
    reason: string | null;
    occurredAt: string;
};

export type DepositSagaState = "AwaitingSettlement" | "Completed" | "Compensating" | "Compensated";

export type DepositSaga = {
    depositId: string;
    state: DepositSagaState;
    amount: number;
    currency: string;
    cardBrand: string;
    cardLast4: string;
    failureReason: string | null;
    startedAt: string;
    updatedAt: string;
    steps: { name: string; occurredAt: string; detail: string | null }[];
};

export const isSagaRunning = (saga: DepositSaga) => saga.state === "AwaitingSettlement" || saga.state === "Compensating";

export type StoredEvent = {
    position: number;
    version: number;
    eventId: string;
    type: string;
    occurredAt: string;
    data: Record<string, unknown>;
};

export type WalletEventStream = {
    streamId: string;
    version: number;
    snapshotInterval: number;
    snapshot: { version: number; schemaVersion: number; createdAt: string; state: Record<string, unknown> } | null;
    eventsSinceSnapshot: number;
    events: StoredEvent[];
};

export type DepositRequest = {
    depositId: string;
    amount: number;
    card: { holderName: string; number: string; expiryMonth: number; expiryYear: number; cvv: string };
    locale: "en" | "tr";
};

export type DepositResult = {
    depositId: string;
    amount: number;
    currency: string;
    balance: number;
    cardBrand: string;
    cardLast4: string;
    authorizationCode: string;
    walletVersion: number;
};

export const minDeposit = 1;
export const maxDeposit = 10_000;

export const walletKey = ["wallet"] as const;

export const walletQueryOptions = queryOptions({
    queryKey: [...walletKey, "balance"],
    queryFn: async ({ signal }) => (await http.get<Wallet>("/api/v1/payments/wallet", { signal })).data
});

export const walletTransactionsQueryOptions = (page: number, pageSize = 10) =>
    queryOptions({
        queryKey: [...walletKey, "transactions", { page, pageSize }],
        queryFn: async ({ signal }) =>
            (await http.get<Page<WalletTransaction>>("/api/v1/payments/wallet/transactions", { params: { page, pageSize }, signal })).data
    });

export const walletEventsQueryOptions = queryOptions({
    queryKey: [...walletKey, "events"],
    queryFn: async ({ signal }) => (await http.get<WalletEventStream>("/api/v1/payments/wallet/events", { signal })).data
});

export const walletSagasQueryOptions = queryOptions({
    queryKey: [...walletKey, "sagas"],
    queryFn: async ({ signal }) => (await http.get<DepositSaga[]>("/api/v1/payments/wallet/sagas", { signal })).data,
    refetchInterval: query => (query.state.data?.some(isSagaRunning) ? 1500 : false)
});

export async function depositFunds(request: DepositRequest) {
    return (await http.post<DepositResult>("/api/v1/payments/wallet/deposits", request)).data;
}
