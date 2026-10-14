import { useQuery, useQueryClient } from "@tanstack/react-query";
import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { CreditCard, FlaskConical, Loader2, Lock, MapPin, PackageX, TriangleAlert, WalletCards } from "lucide-react";
import { useState, type FormEvent } from "react";
import { useAppDispatch } from "@/app/hooks";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Separator } from "@/components/ui/separator";
import { cleared } from "@/features/cart/cart-slice";
import { useCartLines } from "@/features/cart/use-cart-lines";
import { ProductImage } from "@/features/catalog/components/product-image";
import { authorizeCardPayment, ordersKey, placeOrder, type PaymentMethod, type ShippingAddress } from "@/features/orders/orders-api";
import {
    digitsOnly,
    failingSettlementTestCard,
    fieldOf,
    parseExpiry,
    testCard,
    validateCard,
    type CardField,
    type CardForm
} from "@/features/wallet/card";
import { CardFields, Field } from "@/features/wallet/components/card-fields";
import { walletKey, walletQueryOptions } from "@/features/wallet/wallet-api";
import type { MessageKey } from "@/i18n/messages";
import { useTranslation } from "@/i18n/use-translation";
import { formatPrice } from "@/lib/format";
import { ApiError } from "@/lib/problem-details";
import { cn } from "@/lib/utils";

export const Route = createFileRoute("/_authenticated/checkout")({
    component: CheckoutPage
});

type AddressField = keyof ShippingAddress;
const addressFields: AddressField[] = ["fullName", "line1", "line2", "city", "postalCode", "country", "phone"];

function validateAddress(address: ShippingAddress): Partial<Record<AddressField, MessageKey>> {
    const errors: Partial<Record<AddressField, MessageKey>> = {};
    const length = (value: string | null | undefined, min: number, max: number) => (value ?? "").trim().length >= min && (value ?? "").trim().length <= max;
    if (!length(address.fullName, 2, 100)) errors.fullName = "checkout.error.length";
    if (!length(address.line1, 3, 200)) errors.line1 = "checkout.error.length";
    if (!length(address.line2, 0, 200)) errors.line2 = "checkout.error.length";
    if (!length(address.city, 2, 100)) errors.city = "checkout.error.length";
    if (!length(address.postalCode, 3, 20)) errors.postalCode = "checkout.error.length";
    if (!length(address.country, 2, 56)) errors.country = "checkout.error.length";
    if (!/^\+?[0-9 ()-]{7,20}$/.test(address.phone.trim())) errors.phone = "order.checkout.error.phone";
    return errors;
}

function CheckoutPage() {
    const { session } = Route.useRouteContext();
    const { t, locale } = useTranslation();
    const navigate = useNavigate();
    const dispatch = useAppDispatch();
    const queryClient = useQueryClient();
    const { items, lines, subtotal, currency, shortages, isPending } = useCartLines();
    const wallet = useQuery(walletQueryOptions);

    const [address, setAddress] = useState<ShippingAddress>({
        fullName: session.name,
        line1: "",
        line2: "",
        city: "",
        postalCode: "",
        country: locale === "tr" ? "Türkiye" : "",
        phone: ""
    });
    const [addressErrors, setAddressErrors] = useState<Partial<Record<AddressField, MessageKey>>>({});
    const [method, setMethod] = useState<PaymentMethod>("wallet");
    const [card, setCard] = useState<CardForm>({ holderName: session.name, number: "", expiry: "", cvv: "" });
    const [cardErrors, setCardErrors] = useState<Partial<Record<CardField, string>>>({});
    const [lostParcel, setLostParcel] = useState(false);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string>();

    const balance = wallet.data?.balance ?? 0;
    const walletShort = wallet.data !== undefined && balance < subtotal;

    async function submit(event: FormEvent) {
        event.preventDefault();
        setError(undefined);
        const foundAddress = validateAddress(address);
        const foundCard = method === "card" ? validateCard(card) : {};
        setAddressErrors(foundAddress);
        setCardErrors(Object.fromEntries(Object.entries(foundCard).map(([key, code]) => [key, t(cardMessage(code))])));
        if (Object.keys(foundAddress).length > 0 || Object.keys(foundCard).length > 0 || lines.length === 0) {
            return;
        }

        setSubmitting(true);
        try {
            let cardPayment: Awaited<ReturnType<typeof authorizeCardPayment>> | undefined;
            if (method === "card") {
                const expiry = parseExpiry(card.expiry)!;
                cardPayment = await authorizeCardPayment({
                    amount: subtotal,
                    card: { holderName: card.holderName.trim(), number: digitsOnly(card.number), expiryMonth: expiry.month, expiryYear: expiry.year, cvv: card.cvv }
                });
            }

            const order = await placeOrder({
                lines: items.map(item => ({ productId: item.productId, quantity: item.quantity })),
                shippingAddress: { ...address, line2: address.line2?.trim() || null },
                paymentMethod: method,
                cardPaymentId: cardPayment?.id,
                cardBrand: cardPayment?.cardBrand,
                cardLast4: cardPayment?.cardLast4,
                scenario: lostParcel ? "lost_parcel" : undefined,
                locale
            });

            dispatch(cleared());
            await queryClient.invalidateQueries({ queryKey: ordersKey });
            await queryClient.invalidateQueries({ queryKey: walletKey });
            await navigate({ to: "/orders/$orderId", params: { orderId: order.id }, search: { placed: true } });
        } catch (caught) {
            if (caught instanceof ApiError && caught.problem.errors !== undefined) {
                const fromServer: Partial<Record<CardField, string>> = {};
                for (const [field, codes] of Object.entries(caught.problem.errors)) {
                    const key = fieldOf(field);
                    if (key !== undefined && codes[0] !== undefined) fromServer[key] = t(cardMessage(codes[0]));
                }
                setCardErrors(fromServer);
                setError(t("order.checkout.error.validation"));
            } else {
                setError(t(caught instanceof ApiError && caught.code === "orders.product_unavailable" ? "order.checkout.error.productUnavailable" : "order.checkout.error.unexpected"));
            }
        } finally {
            setSubmitting(false);
        }
    }

    if (items.length === 0 && !submitting) {
        return (
            <section className="mx-auto flex max-w-xl flex-col items-center gap-4 px-4 py-24 text-center">
                <h1 className="text-2xl font-semibold tracking-tight">{t("cart.empty.title")}</h1>
                <Button asChild size="lg">
                    <Link to="/products" search={{ page: 1 }}>
                        {t("home.hero.primary")}
                    </Link>
                </Button>
            </section>
        );
    }

    return (
        <form onSubmit={submit} noValidate className="mx-auto max-w-6xl px-4 py-10 sm:px-6 lg:px-8">
            <h1 className="text-2xl font-semibold tracking-tight">{t("order.checkout.title")}</h1>
            <p className="text-sm text-muted-foreground">{t("order.checkout.subtitle")}</p>

            <div className="mt-6 grid gap-6 lg:grid-cols-[1fr_24rem]">
                <div className="flex flex-col gap-6">
                    <Card>
                        <CardHeader>
                            <CardTitle className="flex items-center gap-2">
                                <span className="flex size-6 items-center justify-center rounded-full bg-primary text-xs text-primary-foreground">1</span>
                                <MapPin className="size-4 text-primary" /> {t("order.checkout.address")}
                            </CardTitle>
                        </CardHeader>
                        <CardContent className="grid gap-4 sm:grid-cols-2">
                            {addressFields.map(field => (
                                <div key={field} className={cn(field === "line1" || field === "line2" || field === "fullName" ? "sm:col-span-2" : undefined)}>
                                    <Field id={`address-${field}`} label={t(`order.checkout.field.${field}` as MessageKey)} error={addressErrors[field] && t(addressErrors[field])}>
                                        <Input
                                            id={`address-${field}`}
                                            value={address[field] ?? ""}
                                            autoComplete={autoComplete[field]}
                                            onChange={event => {
                                                setAddress(current => ({ ...current, [field]: event.target.value }));
                                                setAddressErrors(current => ({ ...current, [field]: undefined }));
                                            }}
                                            aria-invalid={addressErrors[field] !== undefined}
                                        />
                                    </Field>
                                </div>
                            ))}
                        </CardContent>
                    </Card>

                    <Card>
                        <CardHeader>
                            <CardTitle className="flex items-center gap-2">
                                <span className="flex size-6 items-center justify-center rounded-full bg-primary text-xs text-primary-foreground">2</span>
                                {t("order.checkout.payment")}
                            </CardTitle>
                            <CardDescription>{t("order.checkout.paymentText")}</CardDescription>
                        </CardHeader>
                        <CardContent className="flex flex-col gap-4">
                            <div role="radiogroup" className="grid gap-3 sm:grid-cols-2">
                                <MethodOption
                                    selected={method === "wallet"}
                                    onSelect={() => setMethod("wallet")}
                                    icon={<WalletCards className="size-5" />}
                                    title={t("order.checkout.wallet")}
                                    text={
                                        wallet.data === undefined
                                            ? "…"
                                            : t("order.checkout.walletBalance", { balance: formatPrice(balance, wallet.data.currency, locale) })
                                    }
                                    warning={walletShort}
                                />
                                <MethodOption
                                    selected={method === "card"}
                                    onSelect={() => setMethod("card")}
                                    icon={<CreditCard className="size-5" />}
                                    title={t("order.checkout.card")}
                                    text={t("order.checkout.cardText")}
                                />
                            </div>

                            {method === "wallet" && walletShort && (
                                <p className="flex flex-wrap items-center gap-2 rounded-lg bg-amber-50 px-3 py-2 text-xs text-amber-900">
                                    <TriangleAlert className="size-4 shrink-0" />
                                    {t("order.checkout.walletShort")}
                                    <Link to="/wallet" search={{}} className="font-medium underline">
                                        {t("wallet.topUp.title")}
                                    </Link>
                                </p>
                            )}

                            {method === "card" && (
                                <div className="flex flex-col gap-4 rounded-xl border p-4">
                                    <div className="flex flex-wrap gap-2">
                                        <Button type="button" variant="outline" size="sm" onClick={() => (setCard(testCard(card.holderName || session.name)), setCardErrors({}))}>
                                            {t("checkout.testMode.fill")}
                                        </Button>
                                        <Button
                                            type="button"
                                            variant="outline"
                                            size="sm"
                                            className="border-red-300 text-red-700 hover:bg-red-50 hover:text-red-800"
                                            onClick={() => (setCard(failingSettlementTestCard(card.holderName || session.name)), setCardErrors({}))}
                                        >
                                            <TriangleAlert /> {t("order.checkout.fillDeclined")}
                                        </Button>
                                    </div>
                                    <CardFields
                                        form={card}
                                        errors={cardErrors}
                                        onChange={(field, value) => {
                                            setCard(current => ({ ...current, [field]: value }));
                                            setCardErrors(current => ({ ...current, [field]: undefined }));
                                        }}
                                    />
                                </div>
                            )}
                        </CardContent>
                    </Card>

                    <Alert className="border-amber-300 bg-amber-50 text-amber-900">
                        <FlaskConical />
                        <AlertTitle>{t("order.checkout.scenarios.title")}</AlertTitle>
                        <AlertDescription className="flex flex-col gap-3 text-amber-900/90">
                            <p>{t("order.checkout.scenarios.text")}</p>
                            <ul className="list-disc space-y-1 pl-5 text-xs">
                                <li>{t("order.checkout.scenarios.outOfStock")}</li>
                                <li>{t("order.checkout.scenarios.noFunds")}</li>
                                <li>{t("order.checkout.scenarios.cardDeclined")}</li>
                            </ul>
                            <label className="flex cursor-pointer items-start gap-2 rounded-lg border border-amber-300 bg-white px-3 py-2 text-sm text-amber-950">
                                <input type="checkbox" className="mt-1 accent-amber-600" checked={lostParcel} onChange={event => setLostParcel(event.target.checked)} />
                                <span>
                                    <span className="flex items-center gap-1.5 font-medium">
                                        <PackageX className="size-4" /> {t("order.checkout.scenarios.lostParcel")}
                                    </span>
                                    <span className="text-xs">{t("order.checkout.scenarios.lostParcelText")}</span>
                                </span>
                            </label>
                        </AlertDescription>
                    </Alert>
                </div>

                <Card className="h-fit lg:sticky lg:top-24">
                    <CardHeader>
                        <CardTitle>{t("checkout.summary")}</CardTitle>
                    </CardHeader>
                    <CardContent className="flex flex-col gap-4 text-sm">
                        <ul className="flex flex-col gap-3">
                            {lines.map(line => (
                                <li key={line.productId} className="flex items-center gap-3">
                                    <div className="relative">
                                        <ProductImage src={line.imageUrl} alt={line.name} className="size-12 rounded-lg border" />
                                        <span className="absolute -top-1.5 -right-1.5 flex size-5 items-center justify-center rounded-full bg-primary text-[10px] font-semibold text-primary-foreground">
                                            {line.quantity}
                                        </span>
                                    </div>
                                    <span className="line-clamp-2 flex-1">{line.name}</span>
                                    <span className="font-medium">{formatPrice(line.lineTotal, line.currency, locale)}</span>
                                </li>
                            ))}
                        </ul>
                        <Separator />
                        <div className="flex justify-between">
                            <span className="text-muted-foreground">{t("cart.subtotal")}</span>
                            <span className="font-medium">{formatPrice(subtotal, currency, locale)}</span>
                        </div>
                        <div className="flex justify-between">
                            <span className="text-muted-foreground">{t("cart.shipping")}</span>
                            <span className="font-medium">{t("cart.free")}</span>
                        </div>
                        <Separator />
                        <div className="flex justify-between text-base font-semibold">
                            <span>{t("checkout.total")}</span>
                            <span>{formatPrice(subtotal, currency, locale)}</span>
                        </div>
                        <p className="text-xs text-muted-foreground">{t("cart.vatIncluded")}</p>
                        {shortages.length > 0 && (
                            <p className="flex gap-2 rounded-lg bg-amber-50 px-3 py-2 text-xs text-amber-900">
                                <TriangleAlert className="size-4 shrink-0" />
                                {t("cart.shortageWarning")}
                            </p>
                        )}
                        {error !== undefined && <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>}
                        <Button type="submit" size="lg" disabled={submitting || isPending || lines.length === 0}>
                            {submitting ? <Loader2 className="animate-spin" /> : <Lock />}
                            {t("order.checkout.place", { amount: formatPrice(subtotal, currency, locale) })}
                        </Button>
                        <p className="text-xs text-muted-foreground">{t("order.checkout.note")}</p>
                    </CardContent>
                </Card>
            </div>
        </form>
    );
}

const autoComplete: Record<AddressField, string> = {
    fullName: "name",
    line1: "address-line1",
    line2: "address-line2",
    city: "address-level2",
    postalCode: "postal-code",
    country: "country-name",
    phone: "tel"
};

function MethodOption({
    selected,
    onSelect,
    icon,
    title,
    text,
    warning = false
}: {
    selected: boolean;
    onSelect: () => void;
    icon: React.ReactNode;
    title: string;
    text: string;
    warning?: boolean;
}) {
    return (
        <button
            type="button"
            role="radio"
            aria-checked={selected}
            onClick={onSelect}
            className={cn(
                "flex cursor-pointer items-start gap-3 rounded-xl border p-4 text-left transition-colors",
                selected ? "border-primary bg-primary/5 ring-2 ring-primary/30" : "hover:bg-muted/50"
            )}
        >
            <span className={cn("flex size-10 shrink-0 items-center justify-center rounded-lg", selected ? "bg-primary text-primary-foreground" : "bg-muted text-muted-foreground")}>
                {icon}
            </span>
            <span className="flex flex-col">
                <span className="font-medium">{title}</span>
                <span className={cn("text-xs", warning ? "text-amber-700" : "text-muted-foreground")}>{text}</span>
            </span>
            <span className={cn("ml-auto mt-1 size-4 shrink-0 rounded-full border-2", selected ? "border-primary bg-primary shadow-[inset_0_0_0_2px_white]" : "border-muted-foreground/40")} />
        </button>
    );
}

const cardMessages: Record<string, MessageKey> = {
    "validation.required": "checkout.error.required",
    "validation.length": "checkout.error.length",
    "validation.card_number": "checkout.error.cardNumber",
    "validation.card_expiry": "checkout.error.cardExpiry",
    "validation.card_expired": "checkout.error.cardExpired",
    "validation.card_cvv": "checkout.error.cardCvv"
};

function cardMessage(code: string): MessageKey {
    return cardMessages[code] ?? "checkout.error.unexpected";
}
