import { Link, useRouterState } from "@tanstack/react-router";
import { Menu, ShoppingBag } from "lucide-react";
import { useState } from "react";
import { useAppSelector } from "@/app/hooks";
import { Button } from "@/components/ui/button";
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from "@/components/ui/sheet";
import { UserMenu } from "@/features/auth/components/user-menu";
import { selectItemCount } from "@/features/cart/cart-slice";
import { ProductSearchForm } from "@/features/catalog/components/product-search-form";
import { useTranslation } from "@/i18n/use-translation";
import { Brand } from "./brand";
import { LanguageSwitcher } from "./language-switcher";

export function SiteHeader() {
    const { t } = useTranslation();
    const cartCount = useAppSelector(state => selectItemCount(state));
    const [menuOpen, setMenuOpen] = useState(false);
    const headerQuery = useRouterState({
        select: state => (state.location.pathname === "/products" ? String((state.location.search as { q?: string }).q ?? "") : "")
    });

    const navigation = (
        <>
            <Link
                to="/"
                activeOptions={{ exact: true }}
                className="rounded-lg px-3 py-2 text-sm font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground data-[status=active]:text-foreground"
            >
                {t("nav.home")}
            </Link>
            <Link
                to="/products"
                search={{ page: 1 }}
                className="rounded-lg px-3 py-2 text-sm font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground data-[status=active]:text-foreground"
            >
                {t("nav.catalog")}
            </Link>
        </>
    );

    return (
        <header className="sticky top-0 z-40 border-b bg-background/85 backdrop-blur supports-backdrop-filter:bg-background/70">
            <div className="mx-auto flex h-16 max-w-7xl items-center gap-4 px-4 sm:px-6 lg:px-8">
                <Sheet open={menuOpen} onOpenChange={setMenuOpen}>
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
                        <ProductSearchForm initialQuery={headerQuery} className="px-4" onSearched={() => setMenuOpen(false)} />
                        <nav className="flex flex-col gap-1 px-4">{navigation}</nav>
                    </SheetContent>
                </Sheet>

                <Brand className="shrink-0" />

                <nav className="hidden items-center gap-1 md:flex">{navigation}</nav>

                <ProductSearchForm initialQuery={headerQuery} className="ml-auto hidden max-w-md flex-1 lg:block" />

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
