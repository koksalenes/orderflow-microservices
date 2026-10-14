import { useQuery } from "@tanstack/react-query";
import { createFileRoute, Link } from "@tanstack/react-router";
import { ArrowLeft, CircleCheck, CircleX, CreditCard, FileText, GitBranch, MapPin, PackageCheck, Truck, Undo2, WalletCards } from "lucide-react";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { Skeleton } from "@/components/ui/skeleton";
import { ProductImage } from "@/features/catalog/components/product-image";
import { OrderProgress } from "@/features/orders/components/order-progress";
import { OrderStatusBadge } from "@/features/orders/components/order-status-badge";
import { OrderTimeline } from "@/features/orders/components/order-timeline";
import { reasonKey } from "@/features/orders/order-labels";
import { orderQueryOptions, type Order } from "@/features/orders/orders-api";
import { useTranslation } from "@/i18n/use-translation";
import { formatDateTime, formatPrice } from "@/lib/format";
import { ApiError } from "@/lib/problem-details";

type OrderSearch = { placed?: boolean };

export const Route = createFileRoute("/_authenticated/orders/$orderId")({
    validateSearch: (search: Record<string, unknown>): OrderSearch => (search.placed === true || search.placed === "true" ? { placed: true } : {}),
    component: OrderPage
});

function OrderPage() {
    const { orderId } = Route.useParams();
    const { placed } = Route.useSearch();
    const { t } = useTranslation();
    const { data: order, isPending, error } = useQuery(orderQueryOptions(orderId));

    return (
        <div className="mx-auto max-w-6xl px-4 py-8 sm:px-6 lg:px-8">
            <Button asChild variant="ghost" className="-ml-2.5 text-muted-foreground">
                <Link to="/orders" search={{ page: 1 }}>
                    <ArrowLeft /> {t("orders.back")}
                </Link>
            </Button>

            {isPending ? (
                <Skeleton className="mt-4 h-96 w-full rounded-2xl" />
            ) : error !== null ? (
                <p className="mt-6 text-sm text-destructive">
                    {error instanceof ApiError && error.status === 404 ? t("orders.notFound") : t("orders.loadError")}
                </p>
            ) : (
                <OrderDetails order={order} placed={placed === true} />
            )}
        </div>
    );
}

function OrderDetails({ order, placed }: { order: Order; placed: boolean }) {
    const { t, locale } = useTranslation();
    const money = (value: number) => formatPrice(value, order.currency, locale);
    const address = order.shippingAddress;
    const failed = order.status === "Cancelled" || order.status === "Refunded" || order.status === "Refunding";

    return (
        <>
            <div className="mt-2 flex flex-wrap items-end justify-between gap-4">
                <div>
                    <p className="text-sm text-muted-foreground">{t("orders.order")}</p>
                    <h1 className="flex flex-wrap items-center gap-3 font-mono text-2xl font-semibold tracking-tight">
                        {order.number} <OrderStatusBadge status={order.status} className="font-sans" />
                    </h1>
                    <p className="mt-1 text-sm text-muted-foreground">{t("orders.placedAt", { date: formatDateTime(order.placedAt, locale) })}</p>
                </div>
                <p className="text-2xl font-semibold">{money(order.total)}</p>
            </div>

            {placed && !failed && (
                <Alert className="mt-6 border-emerald-200 bg-emerald-50 text-emerald-800">
                    <CircleCheck />
                    <AlertTitle>{t("orders.placedBanner.title")}</AlertTitle>
                    <AlertDescription className="text-emerald-800/90">{t("orders.placedBanner.text")}</AlertDescription>
                </Alert>
            )}

            {failed ? (
                <Alert className={order.status === "Cancelled" ? "mt-6 border-red-200 bg-red-50 text-red-800" : "mt-6 border-amber-300 bg-amber-50 text-amber-900"}>
                    {order.status === "Cancelled" ? <CircleX /> : <Undo2 />}
                    <AlertTitle>{t(order.status === "Cancelled" ? "orders.failed.cancelled" : "orders.failed.refunded")}</AlertTitle>
                    <AlertDescription className="text-current/90">
                        <p>{t(reasonKey(order.cancellationReason))}</p>
                        <p className="mt-1 text-xs">{t(order.status === "Cancelled" ? "orders.failed.cancelledText" : "orders.failed.refundedText")}</p>
                    </AlertDescription>
                </Alert>
            ) : (
                <Card className="mt-6">
                    <CardContent className="py-2">
                        <OrderProgress status={order.status} />
                    </CardContent>
                </Card>
            )}

            <div className="mt-6 grid gap-6 lg:grid-cols-[1fr_22rem]">
                <div className="flex flex-col gap-6">
                    <Card>
                        <CardHeader>
                            <CardTitle>{t("orders.items.title")}</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <ul className="divide-y">
                                {order.lines.map(line => (
                                    <li key={line.productId} className="flex items-center gap-4 py-3 first:pt-0 last:pb-0">
                                        <Link to="/products/$productId" params={{ productId: line.productId }}>
                                            <ProductImage src={line.imageUrl} alt={line.name} className="size-16 rounded-xl border" />
                                        </Link>
                                        <div className="min-w-0 flex-1">
                                            <p className="line-clamp-2 font-medium">{line.name}</p>
                                            <p className="text-xs text-muted-foreground">
                                                {line.quantity} × {money(line.unitPrice)}
                                            </p>
                                        </div>
                                        <p className="font-semibold">{money(line.lineTotal)}</p>
                                    </li>
                                ))}
                            </ul>
                            <Separator className="my-4" />
                            <div className="flex flex-col gap-2 text-sm">
                                <div className="flex justify-between">
                                    <span className="text-muted-foreground">{t("cart.subtotal")}</span>
                                    <span>{money(order.subtotal)}</span>
                                </div>
                                <div className="flex justify-between">
                                    <span className="text-muted-foreground">{t("cart.shipping")}</span>
                                    <span>{order.shippingFee === 0 ? t("cart.free") : money(order.shippingFee)}</span>
                                </div>
                                <div className="flex justify-between text-base font-semibold">
                                    <span>{t("checkout.total")}</span>
                                    <span>{money(order.total)}</span>
                                </div>
                            </div>
                        </CardContent>
                    </Card>

                    <div className="grid gap-6 sm:grid-cols-2">
                        <Card>
                            <CardHeader>
                                <CardTitle className="flex items-center gap-2 text-base">
                                    <MapPin className="size-4 text-primary" /> {t("order.checkout.address")}
                                </CardTitle>
                            </CardHeader>
                            <CardContent className="text-sm leading-6 text-muted-foreground">
                                <p className="font-medium text-foreground">{address.fullName}</p>
                                <p>{address.line1}</p>
                                {address.line2 && <p>{address.line2}</p>}
                                <p>
                                    {address.postalCode} {address.city}
                                </p>
                                <p>{address.country}</p>
                                <p>{address.phone}</p>
                            </CardContent>
                        </Card>
                        <Card>
                            <CardHeader>
                                <CardTitle className="flex items-center gap-2 text-base">
                                    {order.paymentMethod === "card" ? <CreditCard className="size-4 text-primary" /> : <WalletCards className="size-4 text-primary" />}
                                    {t("order.checkout.payment")}
                                </CardTitle>
                            </CardHeader>
                            <CardContent className="flex flex-col gap-3 text-sm">
                                <p>
                                    {order.paymentMethod === "card"
                                        ? `${order.cardBrand ?? t("order.checkout.card")} •••• ${order.cardLast4 ?? "????"}`
                                        : t("order.checkout.wallet")}
                                </p>
                                {order.trackingNumber !== null && (
                                    <Info icon={<Truck className="size-4" />} label={t("orders.tracking")} value={order.trackingNumber} />
                                )}
                                {order.invoiceNumber !== null && (
                                    <Info icon={<FileText className="size-4" />} label={t("orders.invoice")} value={order.invoiceNumber} />
                                )}
                                {order.scenario === "lost_parcel" && (
                                    <p className="rounded-lg bg-amber-50 px-3 py-2 text-xs text-amber-900">{t("orders.scenarioLostParcel")}</p>
                                )}
                            </CardContent>
                        </Card>
                    </div>
                </div>

                <Card className="h-fit">
                    <CardHeader>
                        <CardTitle className="flex items-center gap-2">
                            <GitBranch className="size-4 text-primary" /> {t("orders.saga.title")}
                        </CardTitle>
                        <CardDescription>{t("orders.saga.text")}</CardDescription>
                    </CardHeader>
                    <CardContent>
                        <OrderTimeline order={order} />
                        {order.status === "Completed" && (
                            <p className="mt-4 flex items-center gap-2 rounded-lg bg-emerald-50 px-3 py-2 text-xs text-emerald-800">
                                <PackageCheck className="size-4" /> {t("orders.saga.completed")}
                            </p>
                        )}
                    </CardContent>
                </Card>
            </div>
        </>
    );
}

function Info({ icon, label, value }: { icon: React.ReactNode; label: string; value: string }) {
    return (
        <div className="flex items-center gap-2 rounded-lg bg-muted px-3 py-2">
            <span className="text-muted-foreground">{icon}</span>
            <span className="text-xs text-muted-foreground">{label}</span>
            <span className="ml-auto font-mono text-xs font-semibold">{value}</span>
        </div>
    );
}
