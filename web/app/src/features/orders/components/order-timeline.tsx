import { CircleCheck, CircleX, Loader2, Undo2 } from "lucide-react";
import { useTranslation } from "@/i18n/use-translation";
import { formatDateTime } from "@/lib/format";
import { cn } from "@/lib/utils";
import { compensationSteps, failureSteps, stepKeys } from "../order-labels";
import { isOrderActive, type Order } from "../orders-api";

export function OrderTimeline({ order }: { order: Order }) {
    const { t, locale } = useTranslation();
    const steps = [...order.steps].reverse();

    return (
        <ol className="flex flex-col">
            {isOrderActive(order.status) && (
                <li className="relative flex gap-3 pb-4">
                    <span className="absolute top-5 left-[9px] h-full w-px bg-border" />
                    <Loader2 className="relative z-10 size-[19px] shrink-0 animate-spin rounded-full bg-card text-primary" />
                    <p className="text-sm text-muted-foreground">{t("orders.timeline.waiting")}</p>
                </li>
            )}
            {steps.map((step, index) => {
                const failed = failureSteps.has(step.name);
                const compensation = compensationSteps.has(step.name);
                const Icon = failed ? CircleX : compensation ? Undo2 : CircleCheck;
                return (
                    <li key={`${step.name}-${index}`} className="relative flex gap-3 pb-4 last:pb-0">
                        {index < steps.length - 1 && <span className="absolute top-5 left-[9px] h-full w-px bg-border" />}
                        <Icon
                            className={cn(
                                "relative z-10 size-[19px] shrink-0 rounded-full bg-card",
                                failed ? "text-red-600" : compensation ? "text-amber-600" : "text-emerald-600"
                            )}
                        />
                        <div className="min-w-0">
                            <p className="text-sm font-medium">{t(stepKeys[step.name] ?? "orders.step.unknown")}</p>
                            <p className="truncate text-xs text-muted-foreground">
                                {formatDateTime(step.occurredAt, locale, true)}
                                {step.detail !== null && <span className="font-mono"> · {step.detail}</span>}
                            </p>
                        </div>
                    </li>
                );
            })}
        </ol>
    );
}
