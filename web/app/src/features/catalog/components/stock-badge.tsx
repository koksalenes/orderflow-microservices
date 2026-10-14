import { CircleAlert, CircleCheck, CircleX } from "lucide-react";
import { Skeleton } from "@/components/ui/skeleton";
import { useTranslation } from "@/i18n/use-translation";
import { cn } from "@/lib/utils";
import type { StockState } from "../use-stock-levels";

export const lowStockThreshold = 5;

export function StockBadge({ stock, className }: { stock: StockState; className?: string }) {
    const { t } = useTranslation();

    if (stock.status === "loading") {
        return <Skeleton className={cn("h-5 w-20 rounded-full", className)} />;
    }

    const { icon: Icon, text, tone } =
        stock.status === "unknown"
            ? { icon: CircleAlert, text: t("stock.unknown"), tone: "bg-muted text-muted-foreground" }
            : stock.available === 0
              ? { icon: CircleX, text: t("stock.out"), tone: "bg-red-50 text-red-700 ring-red-200" }
              : stock.available <= lowStockThreshold
                ? { icon: CircleAlert, text: t("stock.low", { count: stock.available }), tone: "bg-amber-50 text-amber-700 ring-amber-200" }
                : { icon: CircleCheck, text: t("stock.inStock", { count: stock.available }), tone: "bg-emerald-50 text-emerald-700 ring-emerald-200" };

    return (
        <span className={cn("inline-flex w-fit items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium ring-1 ring-transparent ring-inset", tone, className)}>
            <Icon className="size-3.5" /> {text}
        </span>
    );
}
