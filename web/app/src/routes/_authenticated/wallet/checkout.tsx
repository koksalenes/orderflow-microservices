import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createFileRoute, Link, redirect, useNavigate } from "@tanstack/react-router";
import { ArrowLeft, CreditCard, FlaskConical, Loader2, Lock, TriangleAlert } from "lucide-react";
import { useState, type FormEvent } from "react";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import {
    cvvLength,
    detectBrand,
    digitsOnly,
    fieldOf,
    formatCardNumber,
    formatExpiry,
    parseExpiry,
    testCard,
    failingSettlementTestCard,
    validateCard,
    type CardField,
    type CardForm
} from "@/features/wallet/card";
import { depositFunds, maxDeposit, minDeposit, walletKey } from "@/features/wallet/wallet-api";
import type { MessageKey } from "@/i18n/messages";
import { useTranslation } from "@/i18n/use-translation";
import { formatPrice } from "@/lib/format";
import { ApiError } from "@/lib/problem-details";

type CheckoutSearch = { amount: number };

export const Route = createFileRoute("/_authenticated/wallet/checkout")({
    validateSearch: (search: Record<string, unknown>): CheckoutSearch => ({ amount: Number(search.amount) }),
    beforeLoad: ({ search }) => {
        const valid = Number.isFinite(search.amount) && search.amount >= minDeposit && search.amount <= maxDeposit;
        if (!valid) {
            throw redirect({ to: "/wallet", search: {} });
        }
    },
    component: CheckoutPage
});

const currency = "USD";

function CheckoutPage() {
    const { amount } = Route.useSearch();
    const { session } = Route.useRouteContext();
    const { t, locale } = useTranslation();
    const navigate = useNavigate();
    const queryClient = useQueryClient();

    const [depositId] = useState(() => crypto.randomUUID());
    const [form, setForm] = useState<CardForm>({ holderName: session.name, number: "", expiry: "", cvv: "" });
    const [errors, setErrors] = useState<Partial<Record<CardField, string>>>({});

    const number = digitsOnly(form.number);
    const brand = detectBrand(number);

    const deposit = useMutation({
        mutationFn: depositFunds,
        onSuccess: async result => {
            await queryClient.invalidateQueries({ queryKey: walletKey });
            await navigate({ to: "/wallet", search: { deposited: result.amount } });
        },
        onError: error => {
            if (error instanceof ApiError && error.problem.errors !== undefined) {
                const fieldErrors: Partial<Record<CardField, string>> = {};
                for (const [field, codes] of Object.entries(error.problem.errors)) {
                    const key = fieldOf(field);
                    if (key !== undefined && codes[0] !== undefined) fieldErrors[key] = codes[0];
                }
                setErrors(fieldErrors);
            }
        }
    });

    function update(field: CardField, value: string) {
        setForm(current => ({ ...current, [field]: value }));
        setErrors(current => ({ ...current, [field]: undefined }));
    }

    function submit(event: FormEvent) {
        event.preventDefault();
        const found = validateCard(form);
        setErrors(found);
        const expiry = parseExpiry(form.expiry);
        if (Object.keys(found).length > 0 || expiry === null) {
            return;
        }
        deposit.mutate({
            depositId,
            amount,
            locale,
            card: { holderName: form.holderName.trim(), number, expiryMonth: expiry.month, expiryYear: expiry.year, cvv: form.cvv }
        });
    }

    const errorText = (field: CardField) => {
        const code = errors[field];
        return code === undefined ? undefined : t(errorMessageKey(code));
    };

    const generalError =
        deposit.error instanceof ApiError && deposit.error.problem.errors === undefined ? t(errorMessageKey(deposit.error.code)) : undefined;

    return (
        <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6 lg:px-8">
            <Button asChild variant="ghost" className="-ml-2.5 text-muted-foreground">
                <Link to="/wallet" search={{}}>
                    <ArrowLeft /> {t("checkout.back")}
                </Link>
            </Button>
            <h1 className="mt-2 text-2xl font-semibold tracking-tight">{t("checkout.title")}</h1>

            <Alert className="mt-6 border-amber-300 bg-amber-50 text-amber-900">
                <FlaskConical />
                <AlertTitle>{t("checkout.testMode.title")}</AlertTitle>
                <AlertDescription className="flex flex-col gap-3 text-amber-900/90">
                    <p>{t("checkout.testMode.text")}</p>
                    <div className="flex flex-wrap gap-2">
                        <Button
                            type="button"
                            variant="outline"
                            size="sm"
                            className="w-fit border-amber-400 bg-white text-amber-900 hover:bg-amber-100"
                            onClick={() => {
                                setForm(testCard(form.holderName || session.name));
                                setErrors({});
                            }}
                        >
                            {t("checkout.testMode.fill")}
                        </Button>
                        <Button
                            type="button"
                            variant="outline"
                            size="sm"
                            className="w-fit border-red-300 bg-white text-red-700 hover:bg-red-50 hover:text-red-800"
                            onClick={() => {
                                setForm(failingSettlementTestCard(form.holderName || session.name));
                                setErrors({});
                            }}
                        >
                            <TriangleAlert /> {t("checkout.testMode.fillFailing")}
                        </Button>
                    </div>
                    <p className="text-xs">{t("checkout.testMode.failingText")}</p>
                </AlertDescription>
            </Alert>

            <div className="mt-6 grid gap-6 md:grid-cols-[3fr_2fr]">
                <Card>
                    <CardHeader>
                        <CardTitle className="flex items-center gap-2">
                            <CreditCard className="size-4 text-primary" /> {t("checkout.card")}
                        </CardTitle>
                        <CardDescription>{t("checkout.cardText")}</CardDescription>
                    </CardHeader>
                    <CardContent>
                        <form id="checkout" onSubmit={submit} noValidate className="flex flex-col gap-4">
                            <Field id="holderName" label={t("checkout.holderName")} error={errorText("holderName")}>
                                <Input
                                    id="holderName"
                                    autoComplete="cc-name"
                                    value={form.holderName}
                                    onChange={event => update("holderName", event.target.value)}
                                    aria-invalid={errors.holderName !== undefined}
                                />
                            </Field>
                            <Field id="number" label={t("checkout.number")} error={errorText("number")} hint={number.length > 0 ? brand : undefined}>
                                <Input
                                    id="number"
                                    inputMode="numeric"
                                    autoComplete="cc-number"
                                    placeholder="4242 4242 4242 4242"
                                    value={form.number}
                                    onChange={event => update("number", formatCardNumber(event.target.value))}
                                    aria-invalid={errors.number !== undefined}
                                    className="font-mono"
                                />
                            </Field>
                            <div className="grid grid-cols-2 gap-4">
                                <Field id="expiry" label={t("checkout.expiry")} error={errorText("expiry")}>
                                    <Input
                                        id="expiry"
                                        inputMode="numeric"
                                        autoComplete="cc-exp"
                                        placeholder={t("checkout.expiryPlaceholder")}
                                        value={form.expiry}
                                        onChange={event => update("expiry", formatExpiry(event.target.value))}
                                        aria-invalid={errors.expiry !== undefined}
                                        className="font-mono"
                                    />
                                </Field>
                                <Field id="cvv" label={t("checkout.cvv")} error={errorText("cvv")}>
                                    <Input
                                        id="cvv"
                                        inputMode="numeric"
                                        autoComplete="cc-csc"
                                        placeholder={"•".repeat(cvvLength(number))}
                                        value={form.cvv}
                                        onChange={event => update("cvv", digitsOnly(event.target.value).slice(0, 4))}
                                        aria-invalid={errors.cvv !== undefined}
                                        className="font-mono"
                                    />
                                </Field>
                            </div>
                        </form>
                    </CardContent>
                </Card>

                <Card className="h-fit">
                    <CardHeader>
                        <CardTitle>{t("checkout.summary")}</CardTitle>
                    </CardHeader>
                    <CardContent className="flex flex-col gap-4 text-sm">
                        <div className="flex justify-between">
                            <span className="text-muted-foreground">{t("checkout.topUp")}</span>
                            <span className="font-medium">{formatPrice(amount, currency, locale)}</span>
                        </div>
                        <div className="flex justify-between">
                            <span className="text-muted-foreground">{t("checkout.fee")}</span>
                            <span className="font-medium">{formatPrice(0, currency, locale)}</span>
                        </div>
                        <Separator />
                        <div className="flex justify-between text-base">
                            <span className="font-semibold">{t("checkout.total")}</span>
                            <span className="font-semibold">{formatPrice(amount, currency, locale)}</span>
                        </div>
                        {generalError !== undefined && <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{generalError}</p>}
                        <Button type="submit" form="checkout" size="lg" disabled={deposit.isPending}>
                            {deposit.isPending ? <Loader2 className="animate-spin" /> : <Lock />}
                            {t("checkout.pay", { amount: formatPrice(amount, currency, locale) })}
                        </Button>
                        <p className="text-xs text-muted-foreground">{t("checkout.secureNote")}</p>
                    </CardContent>
                </Card>
            </div>
        </div>
    );
}

function Field({ id, label, error, hint, children }: { id: string; label: string; error?: string; hint?: string; children: React.ReactNode }) {
    return (
        <div className="flex flex-col gap-2">
            <div className="flex items-center justify-between">
                <Label htmlFor={id}>{label}</Label>
                {hint !== undefined && <span className="text-xs font-medium text-muted-foreground">{hint}</span>}
            </div>
            {children}
            {error !== undefined && <p className="text-xs text-destructive">{error}</p>}
        </div>
    );
}

const errorMessages: Record<string, MessageKey> = {
    "validation.required": "checkout.error.required",
    "validation.length": "checkout.error.length",
    "validation.card_number": "checkout.error.cardNumber",
    "validation.card_expiry": "checkout.error.cardExpiry",
    "validation.card_expired": "checkout.error.cardExpired",
    "validation.card_cvv": "checkout.error.cardCvv",
    "payments.balance_limit_exceeded": "checkout.error.balanceLimit",
    "payments.duplicate_deposit": "checkout.error.duplicate",
    "payments.concurrency_conflict": "checkout.error.retry"
};

function errorMessageKey(code: string): MessageKey {
    return errorMessages[code] ?? "checkout.error.unexpected";
}
