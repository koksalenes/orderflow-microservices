import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useTranslation } from "@/i18n/use-translation";
import { cvvLength, detectBrand, digitsOnly, formatCardNumber, formatExpiry, type CardField, type CardForm } from "../card";

export function CardFields({
    form,
    errors,
    onChange
}: {
    form: CardForm;
    errors: Partial<Record<CardField, string>>;
    onChange: (field: CardField, value: string) => void;
}) {
    const { t } = useTranslation();
    const number = digitsOnly(form.number);

    return (
        <div className="flex flex-col gap-4">
            <Field id="card-holder" label={t("checkout.holderName")} error={errors.holderName}>
                <Input
                    id="card-holder"
                    autoComplete="cc-name"
                    value={form.holderName}
                    onChange={event => onChange("holderName", event.target.value)}
                    aria-invalid={errors.holderName !== undefined}
                />
            </Field>
            <Field id="card-number" label={t("checkout.number")} error={errors.number} hint={number.length > 0 ? detectBrand(number) : undefined}>
                <Input
                    id="card-number"
                    inputMode="numeric"
                    autoComplete="cc-number"
                    placeholder="4242 4242 4242 4242"
                    value={form.number}
                    onChange={event => onChange("number", formatCardNumber(event.target.value))}
                    aria-invalid={errors.number !== undefined}
                    className="font-mono"
                />
            </Field>
            <div className="grid grid-cols-2 gap-4">
                <Field id="card-expiry" label={t("checkout.expiry")} error={errors.expiry}>
                    <Input
                        id="card-expiry"
                        inputMode="numeric"
                        autoComplete="cc-exp"
                        placeholder={t("checkout.expiryPlaceholder")}
                        value={form.expiry}
                        onChange={event => onChange("expiry", formatExpiry(event.target.value))}
                        aria-invalid={errors.expiry !== undefined}
                        className="font-mono"
                    />
                </Field>
                <Field id="card-cvv" label={t("checkout.cvv")} error={errors.cvv}>
                    <Input
                        id="card-cvv"
                        inputMode="numeric"
                        autoComplete="cc-csc"
                        placeholder={"•".repeat(cvvLength(number))}
                        value={form.cvv}
                        onChange={event => onChange("cvv", digitsOnly(event.target.value).slice(0, 4))}
                        aria-invalid={errors.cvv !== undefined}
                        className="font-mono"
                    />
                </Field>
            </div>
        </div>
    );
}

export function Field({
    id,
    label,
    error,
    hint,
    children
}: {
    id: string;
    label: string;
    error?: string;
    hint?: string;
    children: React.ReactNode;
}) {
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
