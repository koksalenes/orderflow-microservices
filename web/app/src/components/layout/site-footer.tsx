import { useTranslation } from "@/i18n/use-translation";
import { Brand } from "./brand";

export function SiteFooter() {
    const { t } = useTranslation();

    const columns = [
        { title: t("footer.shop"), links: [t("nav.catalog"), t("nav.cart")] },
        { title: t("footer.account"), links: [t("auth.myAccount"), t("nav.orders")] },
        { title: t("footer.company"), links: [t("footer.about"), t("footer.help")] }
    ];

    return (
        <footer className="border-t bg-muted/40">
            <div className="mx-auto grid max-w-7xl gap-10 px-4 py-12 sm:px-6 md:grid-cols-[2fr_repeat(3,1fr)] lg:px-8">
                <div className="flex flex-col gap-3">
                    <Brand />
                    <p className="max-w-xs text-sm text-muted-foreground">{t("brand.tagline")}</p>
                </div>
                {columns.map(column => (
                    <div key={column.title} className="flex flex-col gap-3">
                        <p className="text-sm font-semibold">{column.title}</p>
                        <ul className="flex flex-col gap-2 text-sm text-muted-foreground">
                            {column.links.map(link => (
                                <li key={link}>
                                    <span className="cursor-default transition-colors hover:text-foreground">{link}</span>
                                </li>
                            ))}
                        </ul>
                    </div>
                ))}
            </div>
            <div className="border-t">
                <div className="mx-auto flex max-w-7xl flex-col items-center justify-between gap-2 px-4 py-6 text-xs text-muted-foreground sm:flex-row sm:px-6 lg:px-8">
                    <p>
                        © {new Date().getFullYear()} OrderFlow. {t("footer.rights")}
                    </p>
                    <div className="flex gap-4">
                        <span>{t("footer.terms")}</span>
                        <span>{t("footer.privacy")}</span>
                    </div>
                </div>
            </div>
        </footer>
    );
}
