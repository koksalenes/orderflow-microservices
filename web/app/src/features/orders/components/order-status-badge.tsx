import { Loader2 } from "lucide-react";
import { useTranslation } from "@/i18n/use-translation";
import { cn } from "@/lib/utils";
import { statusKeys, statusTone } from "../order-labels";
import { isOrderActive, type OrderStatus } from "../orders-api";

export function OrderStatusBadge({ status, className }: { status: OrderStatus; className?: string }) {
    const { t } = useTranslation();

    return (
        <span className={cn("inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset", statusTone[status], className)}>
            {isOrderActive(status) && status !== "Delivered" && <Loader2 className="size-3 animate-spin" />}
            {t(statusKeys[status])}
        </span>
    );
}
