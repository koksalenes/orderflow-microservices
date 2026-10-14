import { useQuery } from "@tanstack/react-query";
import { createFileRoute } from "@tanstack/react-router";
import { CircleCheck, WalletCards } from "lucide-react";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Skeleton } from "@/components/ui/skeleton";
import { EventStreamPanel } from "@/features/wallet/components/event-stream-panel";
import { SagaPanel } from "@/features/wallet/components/saga-panel";
import { useWalletLiveUpdates } from "@/features/wallet/use-wallet-live-updates";
import { TopUpForm } from "@/features/wallet/components/top-up-form";
import { TransactionList } from "@/features/wallet/components/transaction-list";
import { walletQueryOptions } from "@/features/wallet/wallet-api";
import { useTranslation } from "@/i18n/use-translation";
import { formatDateTime, formatPrice } from "@/lib/format";

type WalletSearch = { deposited?: number };

export const Route = createFileRoute("/_authenticated/wallet/")({
    validateSearch: (search: Record<string, unknown>): WalletSearch => {
        const deposited = Number(search.deposited);
        return Number.isFinite(deposited) && deposited > 0 ? { deposited } : {};
    },
    component: WalletPage
});

function WalletPage() {
    const { deposited } = Route.useSearch();
    const { t, locale } = useTranslation();
    const { data: wallet, isPending, isError } = useQuery(walletQueryOptions);
    const sagas = useWalletLiveUpdates();
    const currency = wallet?.currency ?? "USD";

    return (
        <div className="mx-auto max-w-6xl px-4 py-10 sm:px-6 lg:px-8">
            <div className="mb-6">
                <h1 className="text-2xl font-semibold tracking-tight">{t("wallet.title")}</h1>
                <p className="text-sm text-muted-foreground">{t("wallet.subtitle")}</p>
            </div>

            {deposited !== undefined && (
                <Alert className="mb-6 border-emerald-200 bg-emerald-50 text-emerald-800">
                    <CircleCheck />
                    <AlertTitle>{t("wallet.success.title")}</AlertTitle>
                    <AlertDescription className="text-emerald-800/90">
                        {t("wallet.success.text", { amount: formatPrice(deposited, currency, locale) })}
                    </AlertDescription>
                </Alert>
            )}

            <section className="relative overflow-hidden rounded-3xl bg-linear-to-br from-blue-700 via-blue-600 to-sky-500 p-6 text-white sm:p-8">
                <div aria-hidden className="pointer-events-none absolute -top-20 -right-16 size-72 rounded-full bg-white/10 blur-2xl" />
                <div className="relative grid gap-8 md:grid-cols-2 md:items-end">
                    <div>
                        <p className="flex items-center gap-2 text-sm text-white/80">
                            <WalletCards className="size-4" /> {t("wallet.balance")}
                        </p>
                        {isPending ? (
                            <Skeleton className="mt-3 h-12 w-48 bg-white/20" />
                        ) : isError ? (
                            <p className="mt-3 text-sm">{t("wallet.loadError")}</p>
                        ) : (
                            <>
                                <p className="mt-2 text-5xl font-semibold tracking-tight">{formatPrice(wallet.balance, wallet.currency, locale)}</p>
                                <p className="mt-2 text-xs text-white/75">
                                    {wallet.updatedAt === null
                                        ? t("wallet.neverUsed")
                                        : t("wallet.updatedAt", { date: formatDateTime(wallet.updatedAt, locale), version: wallet.version })}
                                </p>
                            </>
                        )}
                    </div>
                    <TopUpForm currency={currency} />
                </div>
            </section>

            <div className="mt-8">
                <SagaPanel sagas={sagas} />
            </div>

            <div className="mt-6 grid gap-6 lg:grid-cols-2">
                <TransactionList />
                <EventStreamPanel />
            </div>
        </div>
    );
}
