import { useAppDispatch } from "@/app/hooks";
import { localeChanged } from "@/features/preferences/preferences-slice";
import { locales } from "@/i18n/messages";
import { useTranslation } from "@/i18n/use-translation";
import { cn } from "@/lib/utils";

const labels = { en: "EN", tr: "TR" } as const;

export function LanguageSwitcher() {
    const dispatch = useAppDispatch();
    const { t, locale } = useTranslation();

    return (
        <div role="group" aria-label={t("nav.language")} className="flex items-center gap-0.5 rounded-full border bg-background p-0.5 text-xs shadow-xs">
            {locales.map(code => (
                <button
                    key={code}
                    type="button"
                    aria-pressed={code === locale}
                    onClick={() => dispatch(localeChanged(code))}
                    className={cn(
                        "cursor-pointer rounded-full px-2.5 py-1 font-medium transition-colors",
                        code === locale ? "bg-primary text-primary-foreground" : "text-muted-foreground hover:bg-muted hover:text-foreground"
                    )}
                >
                    {labels[code]}
                </button>
            ))}
        </div>
    );
}
