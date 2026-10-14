import { useQuery } from "@tanstack/react-query";
import { ArrowDownLeft, Loader2, ReceiptText, Undo2 } from "lucide-react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { useTranslation } from "@/i18n/use-translation";
import { formatDateTime, formatPrice } from "@/lib/format";
import { cn } from "@/lib/utils";
import { walletTransactionsQueryOptions, type WalletTransaction } from "../wallet-api";

export function TransactionList() {
    const { t } = useTranslation();
    const { data, isPending, isError } = useQuery(walletTransactionsQueryOptions(1));

    return (
        <Card>
            <CardHeader>
                <CardTitle className="flex items-center gap-2">
                    <ReceiptText className="size-4 text-primary" /> {t("wallet.history.title")}
                </CardTitle>
                <CardDescription>{t("wallet.history.text")}</CardDescription>
            </CardHeader>
            <CardContent>
                {isPending ? (
                    <div className="flex flex-col gap-3">
                        {Array.from({ length: 3 }, (_, index) => (
                            <Skeleton key={index} className="h-12 w-full" />
                        ))}
                    </div>
                ) : isError ? (
                    <p className="text-sm text-destructive">{t("wallet.loadError")}</p>
                ) : data.items.length === 0 ? (
                    <p className="text-sm text-muted-foreground">{t("wallet.history.empty")}</p>
                ) : (
                    <ul className="divide-y">
                        {data.items.map(item => (
                            <TransactionRow key={item.id} item={item} />
                        ))}
                    </ul>
                )}
            </CardContent>
        </Card>
    );
}

function TransactionRow({ item }: { item: WalletTransaction }) {
    const { t, locale } = useTranslation();
    const reversal = item.type === "reversal";
    const reversed = item.status === "reversed";

    return (
        <li className="flex items-center gap-3 py-3 first:pt-0 last:pb-0">
            <span
                className={cn(
                    "flex size-9 shrink-0 items-center justify-center rounded-full",
                    reversal ? "bg-red-50 text-red-700" : "bg-emerald-50 text-emerald-700"
                )}
            >
                {reversal ? <Undo2 className="size-4" /> : <ArrowDownLeft className="size-4" />}
            </span>
            <div className="min-w-0 flex-1">
                <p className="flex flex-wrap items-center gap-x-2 gap-y-1 text-sm font-medium">
                    {reversal ? t("wallet.history.reversal") : t("wallet.history.deposit")}
                    {item.cardLast4 !== null && (
                        <span className="font-normal text-muted-foreground">
                            {item.cardBrand} •••• {item.cardLast4}
                        </span>
                    )}
                    {item.status === "pending" && (
                        <span className="inline-flex items-center gap-1 rounded-full bg-amber-50 px-2 py-0.5 text-xs font-medium text-amber-800">
                            <Loader2 className="size-3 animate-spin" /> {t("wallet.history.status.pending")}
                        </span>
                    )}
                    {reversed && (
                        <span className="rounded-full bg-red-50 px-2 py-0.5 text-xs font-medium text-red-700">{t("wallet.history.status.reversed")}</span>
                    )}
                </p>
                <p className="text-xs text-muted-foreground">{formatDateTime(item.occurredAt, locale)}</p>
            </div>
            <div className="text-right">
                <p
                    className={cn(
                        "text-sm font-semibold",
                        reversal ? "text-red-700" : "text-emerald-700",
                        reversed && "text-muted-foreground line-through"
                    )}
                >
                    {reversal ? "−" : "+"}
                    {formatPrice(Math.abs(item.amount), item.currency, locale)}
                </p>
                <p className="text-xs text-muted-foreground">
                    {t("wallet.history.balanceAfter", { balance: formatPrice(item.balanceAfter, item.currency, locale) })}
                </p>
            </div>
        </li>
    );
}
