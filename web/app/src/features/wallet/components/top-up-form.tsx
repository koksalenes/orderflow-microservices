import { useNavigate } from "@tanstack/react-router";
import { ArrowRight } from "lucide-react";
import { useState, type FormEvent } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useTranslation } from "@/i18n/use-translation";
import { formatPrice } from "@/lib/format";
import { cn } from "@/lib/utils";
import { maxDeposit, minDeposit } from "../wallet-api";

const presets = [25, 50, 100, 250];

export function TopUpForm({ currency }: { currency: string }) {
    const { t, locale } = useTranslation();
    const navigate = useNavigate();
    const [amount, setAmount] = useState("50");

    const value = Number(amount.replace(",", "."));
    const valid = Number.isFinite(value) && value >= minDeposit && value <= maxDeposit && /^\d+([.,]\d{1,2})?$/.test(amount);

    function submit(event: FormEvent) {
        event.preventDefault();
        if (valid) {
            void navigate({ to: "/wallet/checkout", search: { amount: value } });
        }
    }

    return (
        <form onSubmit={submit} className="flex flex-col gap-3">
            <p className="text-sm font-medium text-white/90">{t("wallet.topUp.title")}</p>
            <div className="flex flex-wrap gap-2">
                {presets.map(preset => (
                    <button
                        key={preset}
                        type="button"
                        onClick={() => setAmount(String(preset))}
                        className={cn(
                            "cursor-pointer rounded-full px-3.5 py-1.5 text-sm font-medium ring-1 ring-white/30 transition-colors",
                            value === preset ? "bg-white text-blue-700" : "bg-white/10 text-white hover:bg-white/20"
                        )}
                    >
                        {formatPrice(preset, currency, locale)}
                    </button>
                ))}
            </div>
            <div className="flex flex-col gap-2 sm:flex-row">
                <Input
                    inputMode="decimal"
                    aria-label={t("wallet.topUp.amount")}
                    value={amount}
                    onChange={event => setAmount(event.target.value)}
                    aria-invalid={!valid}
                    className="h-11 border-white/30 bg-white/95 text-slate-900 sm:max-w-40"
                />
                <Button type="submit" size="lg" disabled={!valid} className="h-11 bg-white text-blue-700 hover:bg-blue-50 active:bg-blue-100">
                    {t("wallet.topUp.continue")} <ArrowRight />
                </Button>
            </div>
            <p className="text-xs text-white/75">
                {t("wallet.topUp.limits", { min: formatPrice(minDeposit, currency, locale), max: formatPrice(maxDeposit, currency, locale) })}
            </p>
        </form>
    );
}
