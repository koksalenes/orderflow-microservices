import type { UseQueryResult } from "@tanstack/react-query";
import { CircleCheck, CircleX, GitBranch, Loader2, Undo2 } from "lucide-react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import type { MessageKey } from "@/i18n/messages";
import { useTranslation } from "@/i18n/use-translation";
import { formatDateTime, formatPrice } from "@/lib/format";
import { cn } from "@/lib/utils";
import type { DepositSaga, DepositSagaState } from "../wallet-api";

const stateStyle: Record<DepositSagaState, { key: MessageKey; tone: string; icon: typeof Loader2; spin?: boolean }> = {
    AwaitingSettlement: { key: "wallet.saga.state.awaiting", tone: "bg-amber-50 text-amber-800 ring-amber-200", icon: Loader2, spin: true },
    Completed: { key: "wallet.saga.state.completed", tone: "bg-emerald-50 text-emerald-700 ring-emerald-200", icon: CircleCheck },
    Compensating: { key: "wallet.saga.state.compensating", tone: "bg-red-50 text-red-700 ring-red-200", icon: Loader2, spin: true },
    Compensated: { key: "wallet.saga.state.compensated", tone: "bg-red-50 text-red-700 ring-red-200", icon: Undo2 }
};

const stepKeys: Record<string, MessageKey> = {
    started: "wallet.saga.step.started",
    settlement_requested: "wallet.saga.step.settlementRequested",
    settled: "wallet.saga.step.settled",
    settlement_failed: "wallet.saga.step.settlementFailed",
    wallet_reversed: "wallet.saga.step.walletReversed",
    authorization_voided: "wallet.saga.step.authorizationVoided",
    compensated: "wallet.saga.step.compensated"
};

const failureSteps = new Set(["settlement_failed"]);
const compensationSteps = new Set(["wallet_reversed", "authorization_voided", "compensated"]);

export function SagaPanel({ sagas }: { sagas: UseQueryResult<DepositSaga[]> }) {
    const { t } = useTranslation();

    return (
        <Card>
            <CardHeader>
                <CardTitle className="flex items-center gap-2">
                    <GitBranch className="size-4 text-primary" /> {t("wallet.saga.title")}
                </CardTitle>
                <CardDescription>{t("wallet.saga.text")}</CardDescription>
            </CardHeader>
            <CardContent>
                {sagas.isPending ? (
                    <Skeleton className="h-28 w-full" />
                ) : sagas.isError ? (
                    <p className="text-sm text-destructive">{t("wallet.loadError")}</p>
                ) : sagas.data.length === 0 ? (
                    <p className="text-sm text-muted-foreground">{t("wallet.saga.empty")}</p>
                ) : (
                    <div className="grid gap-4 md:grid-cols-2">
                        {sagas.data.map(saga => (
                            <SagaCard key={saga.depositId} saga={saga} />
                        ))}
                    </div>
                )}
            </CardContent>
        </Card>
    );
}

function SagaCard({ saga }: { saga: DepositSaga }) {
    const { t, locale } = useTranslation();
    const style = stateStyle[saga.state];
    const Icon = style.icon;

    return (
        <div className="rounded-xl border p-4">
            <div className="flex flex-wrap items-start justify-between gap-2">
                <div>
                    <p className="font-semibold">{formatPrice(saga.amount, saga.currency, locale)}</p>
                    <p className="text-xs text-muted-foreground">
                        {saga.cardBrand} •••• {saga.cardLast4} · {formatDateTime(saga.startedAt, locale)}
                    </p>
                </div>
                <span className={cn("inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium ring-1 ring-inset", style.tone)}>
                    <Icon className={cn("size-3.5", style.spin && "animate-spin")} /> {t(style.key)}
                </span>
            </div>
            <ol className="mt-4 flex flex-col gap-0">
                {saga.steps.map((step, index) => {
                    const failed = failureSteps.has(step.name);
                    const compensation = compensationSteps.has(step.name);
                    const StepIcon = failed ? CircleX : compensation ? Undo2 : CircleCheck;
                    return (
                        <li key={`${step.name}-${index}`} className="relative flex gap-3 pb-3 last:pb-0">
                            {index < saga.steps.length - 1 && <span className="absolute top-5 left-[9px] h-full w-px bg-border" />}
                            <StepIcon
                                className={cn(
                                    "relative z-10 size-[19px] shrink-0 rounded-full bg-card",
                                    failed ? "text-red-600" : compensation ? "text-amber-600" : "text-emerald-600"
                                )}
                            />
                            <div className="min-w-0">
                                <p className="text-sm font-medium">{t(stepKeys[step.name] ?? "wallet.saga.step.unknown")}</p>
                                <p className="truncate text-xs text-muted-foreground">
                                    {formatDateTime(step.occurredAt, locale)}
                                    {step.detail !== null && <span className="font-mono"> · {step.detail}</span>}
                                </p>
                            </div>
                        </li>
                    );
                })}
                {saga.state === "AwaitingSettlement" && (
                    <li className="flex gap-3 text-sm text-muted-foreground">
                        <Loader2 className="size-[19px] animate-spin" /> {t("wallet.saga.waiting")}
                    </li>
                )}
            </ol>
        </div>
    );
}
