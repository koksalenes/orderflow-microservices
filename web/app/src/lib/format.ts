import type { Locale } from "@/i18n/messages";

const numberLocales: Record<Locale, string> = { en: "en-US", tr: "tr-TR" };

export function formatPrice(amount: number, currency: string, locale: Locale) {
    return new Intl.NumberFormat(numberLocales[locale], { style: "currency", currency }).format(amount);
}

export function formatDateTime(value: string, locale: Locale, withSeconds = false) {
    return new Intl.DateTimeFormat(numberLocales[locale], { dateStyle: "medium", timeStyle: withSeconds ? "medium" : "short" }).format(
        new Date(value)
    );
}
