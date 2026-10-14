import { Link } from "@tanstack/react-router";
import { Menu, Search, ShoppingBag } from "lucide-react";
import { useAppSelector } from "@/app/hooks";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from "@/components/ui/sheet";
import { UserMenu } from "@/features/auth/components/user-menu";
import { selectItemCount } from "@/features/cart/cart-slice";
import { useTranslation } from "@/i18n/use-translation";
import { Brand } from "./brand";
import { LanguageSwitcher } from "./language-switcher";

export function SiteHeader() {
    const { t } = useTranslation();
    const cartCount = useAppSelector(state => selectItemCount(state));

    const navigation = (
        <>
            <Link
                to="/"
                activeOptions={{ exact: true }}
                className="rounded-lg px-3 py-2 text-sm font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground data-[status=active]:text-foreground"
            >
                {t("nav.home")}
            </Link>
            <span className="flex cursor-not-allowed items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium text-muted-foreground/70">
                {t("nav.catalog")}
                <Badge variant="secondary" className="rounded-full px-1.5 text-[10px]">
                    {t("badge.soon")}
                </Badge>
            </span>
        </>
    );

    return (
        <header className="sticky top-0 z-40 border-b bg-background/85 backdrop-blur supports-backdrop-filter:bg-background/70">
            <div className="mx-auto flex h-16 max-w-7xl items-center gap-4 px-4 sm:px-6 lg:px-8">
                <Sheet>
                    <SheetTrigger asChild>
                        <Button variant="ghost" size="icon-lg" className="md:hidden" aria-label={t("nav.menu")}>
                            <Menu />
                        </Button>
                    </SheetTrigger>
                    <SheetContent side="left" className="w-72">
                        <SheetHeader>
                            <SheetTitle>
                                <Brand />
                            </SheetTitle>
                        </SheetHeader>
                        <nav className="flex flex-col gap-1 px-4">{navigation}</nav>
                    </SheetContent>
                </Sheet>

                <Brand className="shrink-0" />

                <nav className="hidden items-center gap-1 md:flex">{navigation}</nav>

                <div className="relative ml-auto hidden max-w-md flex-1 lg:block">
                    <Search className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
                    <Input type="search" placeholder={t("nav.search")} className="rounded-full bg-muted/60 pl-9 hover:bg-muted" disabled />
                </div>

                <div className="ml-auto flex items-center gap-2 lg:ml-0">
                    <LanguageSwitcher />
                    <Button variant="ghost" size="icon-lg" className="relative" aria-label={t("nav.cart")}>
                        <ShoppingBag />
                        {cartCount > 0 && (
                            <span className="absolute -top-0.5 -right-0.5 flex size-4 items-center justify-center rounded-full bg-primary text-[10px] font-semibold text-primary-foreground">
                                {cartCount}
                            </span>
                        )}
                    </Button>
                    <UserMenu />
                </div>
            </div>
        </header>
    );
}
