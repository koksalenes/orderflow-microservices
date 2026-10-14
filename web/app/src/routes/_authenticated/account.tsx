import { useQuery } from "@tanstack/react-query";
import { createFileRoute, Link } from "@tanstack/react-router";
import { ArrowRight, BadgeCheck, CircleAlert, LogOut, PackageOpen, ShieldCheck, WalletCards } from "lucide-react";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { signOut } from "@/features/auth/session";
import { OrderStatusBadge } from "@/features/orders/components/order-status-badge";
import { myOrdersQueryOptions } from "@/features/orders/orders-api";
import { walletQueryOptions } from "@/features/wallet/wallet-api";
import { formatPrice } from "@/lib/format";
import { useTranslation } from "@/i18n/use-translation";
import { initials } from "@/lib/utils";

export const Route = createFileRoute("/_authenticated/account")({
    component: AccountPage
});

function AccountPage() {
    const { session } = Route.useRouteContext();
    const { t, locale } = useTranslation();
    const wallet = useQuery(walletQueryOptions);

    return (
        <div className="mx-auto max-w-5xl px-4 py-10 sm:px-6 lg:px-8">
            <div className="flex flex-col gap-6 sm:flex-row sm:items-center sm:justify-between">
                <div className="flex items-center gap-4">
                    <Avatar className="size-14">
                        <AvatarFallback className="bg-primary text-lg font-semibold text-primary-foreground">{initials(session.name)}</AvatarFallback>
                    </Avatar>
                    <div>
                        <h1 className="text-2xl font-semibold tracking-tight">{t("account.title")}</h1>
                        <p className="text-sm text-muted-foreground">{t("account.subtitle")}</p>
                    </div>
                </div>
                <Button variant="outline" size="lg" onClick={() => signOut(session)}>
                    <LogOut /> {t("auth.signOut")}
                </Button>
            </div>

            <div className="mt-8 grid gap-6 md:grid-cols-[3fr_2fr]">
                <Card>
                    <CardHeader>
                        <CardTitle>{t("account.profile")}</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <dl className="divide-y text-sm">
                            <Row label={t("account.name")}>{session.name}</Row>
                            <Row label={t("account.email")}>
                                <span className="flex flex-wrap items-center gap-2">
                                    {session.email}
                                    {session.emailVerified ? (
                                        <Badge variant="secondary" className="gap-1 text-emerald-700">
                                            <BadgeCheck className="size-3" /> {t("account.emailVerified")}
                                        </Badge>
                                    ) : (
                                        <Badge variant="secondary" className="gap-1 text-amber-700">
                                            <CircleAlert className="size-3" /> {t("account.emailNotVerified")}
                                        </Badge>
                                    )}
                                </span>
                            </Row>
                            <Row label={t("account.roles")}>
                                <span className="flex flex-wrap gap-1.5">
                                    {session.roles.map(role => (
                                        <Badge key={role} variant="outline">
                                            {role}
                                        </Badge>
                                    ))}
                                </span>
                            </Row>
                        </dl>
                    </CardContent>
                </Card>

                <div className="flex flex-col gap-6">
                    <Card>
                        <CardHeader>
                            <CardTitle className="flex items-center gap-2">
                                <WalletCards className="size-4 text-primary" /> {t("wallet.title")}
                            </CardTitle>
                            <CardDescription>{t("wallet.balance")}</CardDescription>
                        </CardHeader>
                        <CardContent className="flex items-center justify-between gap-4">
                            {wallet.data === undefined ? (
                                <Skeleton className="h-8 w-28" />
                            ) : (
                                <span className="text-2xl font-semibold">{formatPrice(wallet.data.balance, wallet.data.currency, locale)}</span>
                            )}
                            <Button asChild variant="outline">
                                <Link to="/wallet" search={{}}>
                                    {t("wallet.open")} <ArrowRight />
                                </Link>
                            </Button>
                        </CardContent>
                    </Card>
                    <Card>
                        <CardHeader>
                            <CardTitle className="flex items-center gap-2">
                                <ShieldCheck className="size-4 text-primary" /> {t("account.session")}
                            </CardTitle>
                            <CardDescription>{t("account.sessionText")}</CardDescription>
                        </CardHeader>
                    </Card>
                    <RecentOrders />
                </div>
            </div>
        </div>
    );
}

function Row({ label, children }: { label: string; children: React.ReactNode }) {
    return (
        <div className="grid gap-1 py-3 first:pt-0 last:pb-0 sm:grid-cols-[10rem_1fr] sm:gap-4">
            <dt className="text-muted-foreground">{label}</dt>
            <dd className="font-medium">{children}</dd>
        </div>
    );
}

function RecentOrders() {
    const { t, locale } = useTranslation();
    const orders = useQuery(myOrdersQueryOptions(1, 3));

    return (
        <Card>
            <CardHeader>
                <CardTitle className="flex items-center gap-2">
                    <PackageOpen className="size-4 text-primary" /> {t("account.orders")}
                </CardTitle>
                {orders.data?.items.length === 0 && <CardDescription>{t("account.ordersEmpty")}</CardDescription>}
            </CardHeader>
            {orders.data !== undefined && orders.data.items.length > 0 && (
                <CardContent className="flex flex-col gap-2">
                    {orders.data.items.map(order => (
                        <Link
                            key={order.id}
                            to="/orders/$orderId"
                            params={{ orderId: order.id }}
                            search={{}}
                            className="flex items-center gap-3 rounded-lg px-2 py-2 hover:bg-muted"
                        >
                            <span className="font-mono text-xs font-semibold">{order.number}</span>
                            <OrderStatusBadge status={order.status} />
                            <span className="ml-auto text-sm font-medium">{formatPrice(order.total, order.currency, locale)}</span>
                        </Link>
                    ))}
                    <Button asChild variant="outline" size="sm" className="mt-1 w-fit">
                        <Link to="/orders" search={{ page: 1 }}>
                            {t("orders.viewAll")} <ArrowRight />
                        </Link>
                    </Button>
                </CardContent>
            )}
        </Card>
    );
}
