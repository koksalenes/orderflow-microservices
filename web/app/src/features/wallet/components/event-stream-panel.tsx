import { useQuery } from "@tanstack/react-query";
import { Camera, Layers } from "lucide-react";
import { Fragment } from "react";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { useTranslation } from "@/i18n/use-translation";
import { formatDateTime, formatPrice } from "@/lib/format";
import { cn } from "@/lib/utils";
import { walletEventsQueryOptions, type StoredEvent } from "../wallet-api";

export function EventStreamPanel() {
    const { t, locale } = useTranslation();
    const { data, isPending, isError } = useQuery(walletEventsQueryOptions);

    return (
        <Card>
            <CardHeader>
                <CardTitle className="flex items-center gap-2">
                    <Layers className="size-4 text-primary" /> {t("wallet.events.title")}
                </CardTitle>
                <CardDescription>{t("wallet.events.text")}</CardDescription>
            </CardHeader>
            <CardContent className="flex flex-col gap-4">
                {isPending ? (
                    <Skeleton className="h-40 w-full" />
                ) : isError ? (
                    <p className="text-sm text-destructive">{t("wallet.loadError")}</p>
                ) : data.version === 0 ? (
                    <p className="text-sm text-muted-foreground">{t("wallet.events.empty")}</p>
                ) : (
                    <>
                        <dl className="grid grid-cols-2 gap-3 text-sm">
                            <Stat label={t("wallet.events.version")} value={String(data.version)} />
                            <Stat
                                label={t("wallet.events.snapshot")}
                                value={data.snapshot === null ? t("wallet.events.noSnapshot") : `v${data.snapshot.version}`}
                            />
                            <Stat label={t("wallet.events.replayed")} value={String(data.eventsSinceSnapshot)} />
                            <Stat
                                label={t("wallet.events.nextSnapshot")}
                                value={`v${(Math.floor(data.version / data.snapshotInterval) + 1) * data.snapshotInterval}`}
                            />
                        </dl>
                        <p className="rounded-lg bg-muted px-3 py-2 font-mono text-xs break-all text-muted-foreground">{data.streamId}</p>

                        <ol className="flex flex-col gap-2">
                            {data.events.map(event => (
                                <Fragment key={event.eventId}>
                                    <EventRow event={event} locale={locale} />
                                    {data.snapshot !== null && event.version === data.snapshot.version && (
                                        <li className="flex items-center gap-2 py-1 text-xs font-medium text-amber-700">
                                            <span className="h-px flex-1 bg-amber-300" />
                                            <Camera className="size-3.5" />
                                            {t("wallet.events.snapshotLine", { version: data.snapshot.version })}
                                            <span className="h-px flex-1 bg-amber-300" />
                                        </li>
                                    )}
                                </Fragment>
                            ))}
                        </ol>
                    </>
                )}
            </CardContent>
        </Card>
    );
}

function Stat({ label, value }: { label: string; value: string }) {
    return (
        <div className="rounded-lg border px-3 py-2">
            <dt className="text-xs text-muted-foreground">{label}</dt>
            <dd className="text-base font-semibold">{value}</dd>
        </div>
    );
}

const debitEvents = new Set(["wallet.funds_withdrawn.v1", "wallet.deposit_reversed.v1"]);

function EventRow({ event, locale }: { event: StoredEvent; locale: "en" | "tr" }) {
    const amount = typeof event.data.amount === "number" ? event.data.amount : undefined;
    const currency = typeof event.data.currency === "string" ? event.data.currency : "USD";
    const debit = debitEvents.has(event.type);

    return (
        <li className="flex items-start gap-3 rounded-lg border px-3 py-2">
            <Badge variant="secondary" className="mt-0.5 font-mono">
                v{event.version}
            </Badge>
            <div className="min-w-0 flex-1">
                <p className="truncate font-mono text-xs font-medium">{event.type}</p>
                <p className="text-xs text-muted-foreground">{formatDateTime(event.occurredAt, locale)}</p>
            </div>
            {amount !== undefined && (
                <span className={cn("text-sm font-semibold whitespace-nowrap", debit ? "text-red-700" : "text-emerald-700")}>
                    {debit ? "−" : "+"}
                    {formatPrice(amount, currency, locale)}
                </span>
            )}
        </li>
    );
}
