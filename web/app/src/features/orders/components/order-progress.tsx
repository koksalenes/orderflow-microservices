import { Check } from "lucide-react";
import type { MessageKey } from "@/i18n/messages";
import { useTranslation } from "@/i18n/use-translation";
import { cn } from "@/lib/utils";
import type { OrderStatus } from "../orders-api";

const stages: { key: MessageKey; reachedAt: OrderStatus[] }[] = [
    { key: "orders.progress.placed", reachedAt: ["Placed", "AwaitingPayment", "Paid", "Confirmed", "Shipped", "Delivered", "Completed"] },
    { key: "orders.progress.confirmed", reachedAt: ["Confirmed", "Shipped", "Delivered", "Completed"] },
    { key: "orders.progress.shipped", reachedAt: ["Shipped", "Delivered", "Completed"] },
    { key: "orders.progress.delivered", reachedAt: ["Delivered", "Completed"] },
    { key: "orders.progress.invoiced", reachedAt: ["Completed"] }
];

export function OrderProgress({ status }: { status: OrderStatus }) {
    const { t } = useTranslation();
    const current = stages.reduce((last, stage, index) => (stage.reachedAt.includes(status) ? index : last), -1);

    return (
        <ol className="grid grid-cols-5 gap-2">
            {stages.map((stage, index) => {
                const done = index <= current;
                return (
                    <li key={stage.key} className="flex flex-col items-center gap-2 text-center">
                        <div className="flex w-full items-center">
                            <span className={cn("h-1 flex-1 rounded-full", index === 0 ? "bg-transparent" : done ? "bg-primary" : "bg-muted")} />
                            <span
                                className={cn(
                                    "flex size-8 shrink-0 items-center justify-center rounded-full text-xs font-semibold ring-4 ring-background",
                                    done ? "bg-primary text-primary-foreground" : "bg-muted text-muted-foreground",
                                    index === current && index < stages.length - 1 && "animate-pulse"
                                )}
                            >
                                {done ? <Check className="size-4" /> : index + 1}
                            </span>
                            <span className={cn("h-1 flex-1 rounded-full", index === stages.length - 1 ? "bg-transparent" : index < current ? "bg-primary" : "bg-muted")} />
                        </div>
                        <span className={cn("text-xs", done ? "font-medium text-foreground" : "text-muted-foreground")}>{t(stage.key)}</span>
                    </li>
                );
            })}
        </ol>
    );
}
