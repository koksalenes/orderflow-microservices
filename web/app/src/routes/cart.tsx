import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { ArrowRight, Minus, Plus, ShoppingBag, Trash2, TriangleAlert } from "lucide-react";
import { useAppDispatch } from "@/app/hooks";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { Skeleton } from "@/components/ui/skeleton";
import { signIn } from "@/features/auth/session";
import { useSession } from "@/features/auth/use-session";
import { itemRemoved, maxQuantityPerLine, quantitySet } from "@/features/cart/cart-slice";
import { useCartLines, type CartLine } from "@/features/cart/use-cart-lines";
import { ProductImage } from "@/features/catalog/components/product-image";
import { StockBadge } from "@/features/catalog/components/stock-badge";
import { useTranslation } from "@/i18n/use-translation";
import { formatPrice } from "@/lib/format";

export const Route = createFileRoute("/cart")({
    component: CartPage
});

function CartPage() {
    const { t, locale } = useTranslation();
    const { session } = useSession();
    const navigate = useNavigate();
    const { items, lines, subtotal, currency, shortages, isPending } = useCartLines();

    function checkout() {
        if (session === null) {
            signIn({ returnUrl: "/checkout", locale });
            return;
        }
        void navigate({ to: "/checkout" });
    }

    if (items.length === 0) {
        return (
            <section className="mx-auto flex max-w-xl flex-col items-center gap-4 px-4 py-24 text-center">
                <span className="flex size-16 items-center justify-center rounded-full bg-primary/10 text-primary">
                    <ShoppingBag className="size-7" />
                </span>
                <h1 className="text-2xl font-semibold tracking-tight">{t("cart.empty.title")}</h1>
                <p className="text-muted-foreground">{t("cart.empty.text")}</p>
                <Button asChild size="lg">
                    <Link to="/products" search={{ page: 1 }}>
                        {t("home.hero.primary")} <ArrowRight />
                    </Link>
                </Button>
            </section>
        );
    }

    return (
        <div className="mx-auto max-w-6xl px-4 py-10 sm:px-6 lg:px-8">
            <h1 className="text-2xl font-semibold tracking-tight">{t("cart.title")}</h1>
            <p className="text-sm text-muted-foreground">{t("cart.count", { count: items.reduce((sum, item) => sum + item.quantity, 0) })}</p>

            <div className="mt-6 grid gap-6 lg:grid-cols-[1fr_22rem]">
                <Card className="py-0">
                    <ul className="divide-y">
                        {isPending && lines.length === 0
                            ? items.map(item => (
                                  <li key={item.productId} className="flex gap-4 p-4">
                                      <Skeleton className="size-20 rounded-xl" />
                                      <div className="flex flex-1 flex-col gap-2">
                                          <Skeleton className="h-4 w-1/2" />
                                          <Skeleton className="h-4 w-1/4" />
                                      </div>
                                  </li>
                              ))
                            : lines.map(line => <CartRow key={line.productId} line={line} />)}
                    </ul>
                </Card>

                <Card className="h-fit lg:sticky lg:top-24">
                    <CardHeader>
                        <CardTitle>{t("checkout.summary")}</CardTitle>
                    </CardHeader>
                    <CardContent className="flex flex-col gap-3 text-sm">
                        <Row label={t("cart.subtotal")} value={formatPrice(subtotal, currency, locale)} />
                        <Row label={t("cart.shipping")} value={t("cart.free")} />
                        <Separator />
                        <Row label={t("checkout.total")} value={formatPrice(subtotal, currency, locale)} strong />
                        <p className="text-xs text-muted-foreground">{t("cart.vatIncluded")}</p>
                        {shortages.length > 0 && (
                            <p className="flex gap-2 rounded-lg bg-amber-50 px-3 py-2 text-xs text-amber-900">
                                <TriangleAlert className="size-4 shrink-0" />
                                {t("cart.shortageWarning")}
                            </p>
                        )}
                        <Button size="lg" className="mt-2" onClick={checkout} disabled={lines.length === 0}>
                            {t("cart.checkout")} <ArrowRight />
                        </Button>
                        <Button asChild variant="ghost">
                            <Link to="/products" search={{ page: 1 }}>
                                {t("cart.continue")}
                            </Link>
                        </Button>
                    </CardContent>
                </Card>
            </div>
        </div>
    );
}

function CartRow({ line }: { line: CartLine }) {
    const { t, locale } = useTranslation();
    const dispatch = useAppDispatch();
    const tooMany = line.stock.status === "known" && line.stock.available < line.quantity;

    return (
        <li className="flex gap-4 p-4">
            <Link to="/products/$productId" params={{ productId: line.productId }} className="shrink-0">
                <ProductImage src={line.imageUrl} alt={line.name} className="size-20 rounded-xl border sm:size-24" />
            </Link>
            <div className="flex min-w-0 flex-1 flex-col gap-1.5">
                <div className="flex items-start justify-between gap-3">
                    <div className="min-w-0">
                        <p className="text-xs text-muted-foreground uppercase">{line.category}</p>
                        <Link to="/products/$productId" params={{ productId: line.productId }} className="line-clamp-2 font-medium hover:text-primary">
                            {line.name}
                        </Link>
                    </div>
                    <p className="shrink-0 font-semibold">{formatPrice(line.lineTotal, line.currency, locale)}</p>
                </div>
                <p className="text-xs text-muted-foreground">{t("cart.each", { price: formatPrice(line.price, line.currency, locale) })}</p>
                <div className="mt-1 flex flex-wrap items-center gap-3">
                    <div className="flex items-center rounded-lg border">
                        <Button
                            variant="ghost"
                            size="icon"
                            aria-label={t("cart.decrease")}
                            disabled={line.quantity <= 1}
                            onClick={() => dispatch(quantitySet({ productId: line.productId, quantity: line.quantity - 1 }))}
                        >
                            <Minus />
                        </Button>
                        <span className="w-8 text-center text-sm font-medium tabular-nums">{line.quantity}</span>
                        <Button
                            variant="ghost"
                            size="icon"
                            aria-label={t("cart.increase")}
                            disabled={line.quantity >= maxQuantityPerLine}
                            onClick={() => dispatch(quantitySet({ productId: line.productId, quantity: line.quantity + 1 }))}
                        >
                            <Plus />
                        </Button>
                    </div>
                    <StockBadge stock={line.stock} />
                    <Button variant="ghost" size="sm" className="ml-auto text-muted-foreground" onClick={() => dispatch(itemRemoved(line.productId))}>
                        <Trash2 /> {t("cart.remove")}
                    </Button>
                </div>
                {tooMany && <p className="text-xs text-amber-700">{t("cart.moreThanStock")}</p>}
            </div>
        </li>
    );
}

function Row({ label, value, strong = false }: { label: string; value: string; strong?: boolean }) {
    return (
        <div className={strong ? "flex justify-between text-base font-semibold" : "flex justify-between"}>
            <span className={strong ? undefined : "text-muted-foreground"}>{label}</span>
            <span className={strong ? undefined : "font-medium"}>{value}</span>
        </div>
    );
}
