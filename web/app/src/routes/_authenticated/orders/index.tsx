import { useQuery } from "@tanstack/react-query";
import { createFileRoute, Link } from "@tanstack/react-router";
import { ArrowRight, ChevronLeft, ChevronRight, CreditCard, PackageOpen, WalletCards } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { ProductImage } from "@/features/catalog/components/product-image";
import { OrderStatusBadge } from "@/features/orders/components/order-status-badge";
import { reasonKey } from "@/features/orders/order-labels";
import { myOrdersQueryOptions } from "@/features/orders/orders-api";
import { useTranslation } from "@/i18n/use-translation";
import { formatDateTime, formatPrice } from "@/lib/format";

type OrdersSearch = { page: number };

export const Route = createFileRoute("/_authenticated/orders/")({
    validateSearch: (search: Record<string, unknown>): OrdersSearch => {
        const page = Number(search.page);
        return { page: Number.isInteger(page) && page > 0 ? page : 1 };
    },
    component: OrdersPage
});

function OrdersPage() {
    const { page } = Route.useSearch();
    const { t, locale } = useTranslation();
    const { data, isPending, isError } = useQuery(myOrdersQueryOptions(page));
    const pages = data === undefined ? 1 : Math.max(1, Math.ceil(data.totalCount / data.pageSize));

    return (
        <div className="mx-auto max-w-5xl px-4 py-10 sm:px-6 lg:px-8">
            <h1 className="text-2xl font-semibold tracking-tight">{t("orders.title")}</h1>
            <p className="text-sm text-muted-foreground">{t("orders.subtitle")}</p>

            <div className="mt-6 flex flex-col gap-4">
                {isPending ? (
                    Array.from({ length: 3 }, (_, index) => <Skeleton key={index} className="h-28 w-full rounded-xl" />)
                ) : isError ? (
                    <p className="text-sm text-destructive">{t("orders.loadError")}</p>
                ) : data.items.length === 0 ? (
                    <div className="flex flex-col items-center gap-3 rounded-2xl border border-dashed px-6 py-16 text-center">
                        <PackageOpen className="size-10 text-muted-foreground/60" />
                        <p className="font-medium">{t("orders.empty")}</p>
                        <Button asChild>
                            <Link to="/products" search={{ page: 1 }}>
                                {t("home.hero.primary")} <ArrowRight />
                            </Link>
                        </Button>
                    </div>
                ) : (
                    data.items.map(order => (
                        <Link key={order.id} to="/orders/$orderId" params={{ orderId: order.id }} search={{}}>
                            <Card className="flex-row items-center gap-4 p-4 transition-shadow hover:shadow-md">
                                <div className="hidden -space-x-3 sm:flex">
                                    {order.images.slice(0, 3).map(image => (
                                        <ProductImage key={image} src={image} alt="" className="size-14 rounded-xl border-2 border-background shadow-sm" />
                                    ))}
                                </div>
                                <div className="min-w-0 flex-1">
                                    <div className="flex flex-wrap items-center gap-2">
                                        <span className="font-mono text-sm font-semibold">{order.number}</span>
                                        <OrderStatusBadge status={order.status} />
                                    </div>
                                    <p className="mt-1 text-xs text-muted-foreground">
                                        {formatDateTime(order.placedAt, locale)} · {t("orders.items", { count: order.itemCount })}
                                    </p>
                                    {order.cancellationReason !== null && (
                                        <p className="mt-1 text-xs text-red-700">{t(reasonKey(order.cancellationReason))}</p>
                                    )}
                                </div>
                                <div className="text-right">
                                    <p className="font-semibold">{formatPrice(order.total, order.currency, locale)}</p>
                                    <p className="mt-1 flex items-center justify-end gap-1 text-xs text-muted-foreground">
                                        {order.paymentMethod === "card" ? <CreditCard className="size-3.5" /> : <WalletCards className="size-3.5" />}
                                        {order.paymentMethod === "card" ? t("order.checkout.card") : t("order.checkout.wallet")}
                                    </p>
                                </div>
                                <ChevronRight className="size-4 text-muted-foreground" />
                            </Card>
                        </Link>
                    ))
                )}
            </div>

            {pages > 1 && (
                <nav className="mt-8 flex items-center justify-center gap-3">
                    <Button asChild variant="outline" disabled={page <= 1}>
                        <Link to="/orders" search={{ page: page - 1 }} disabled={page <= 1}>
                            <ChevronLeft /> {t("catalog.previous")}
                        </Link>
                    </Button>
                    <span className="text-sm text-muted-foreground">{t("catalog.pageOf", { page, pages })}</span>
                    <Button asChild variant="outline" disabled={page >= pages}>
                        <Link to="/orders" search={{ page: page + 1 }} disabled={page >= pages}>
                            {t("catalog.next")} <ChevronRight />
                        </Link>
                    </Button>
                </nav>
            )}
        </div>
    );
}
