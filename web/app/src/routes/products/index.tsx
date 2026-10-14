import { useQuery } from "@tanstack/react-query";
import { createFileRoute, Link } from "@tanstack/react-router";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { catalogPageSize, productsQueryOptions } from "@/features/catalog/catalog-api";
import { ProductGrid } from "@/features/catalog/components/product-grid";
import { useTranslation } from "@/i18n/use-translation";

type CatalogSearch = { page: number };

export const Route = createFileRoute("/products/")({
    validateSearch: (search: Record<string, unknown>): CatalogSearch => {
        const page = Number(search.page);
        return { page: Number.isInteger(page) && page > 0 ? page : 1 };
    },
    loaderDeps: ({ search }) => ({ page: search.page }),
    loader: ({ context, deps }) => void context.queryClient.prefetchQuery(productsQueryOptions(deps.page)),
    component: CatalogPage
});

function CatalogPage() {
    const { page } = Route.useSearch();
    const { t } = useTranslation();
    const { data, isError, isPlaceholderData } = useQuery(productsQueryOptions(page));

    const pageCount = data === undefined ? 1 : Math.max(1, Math.ceil(data.totalCount / data.pageSize));

    return (
        <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
            <div className="mb-8 flex flex-wrap items-end justify-between gap-4">
                <div>
                    <h1 className="text-3xl font-semibold tracking-tight">{t("catalog.title")}</h1>
                    <p className="mt-1 text-sm text-muted-foreground">
                        {data === undefined ? t("catalog.subtitle") : t("catalog.count", { count: data.totalCount })}
                    </p>
                </div>
            </div>

            {isError ? (
                <Alert variant="destructive">
                    <AlertDescription>{t("catalog.loadError")}</AlertDescription>
                </Alert>
            ) : (
                <div className={isPlaceholderData ? "opacity-60 transition-opacity" : "transition-opacity"}>
                    <ProductGrid products={data?.items} skeletons={catalogPageSize} />
                </div>
            )}

            {data !== undefined && pageCount > 1 && (
                <nav className="mt-10 flex items-center justify-center gap-3" aria-label={t("catalog.pagination")}>
                    <Button asChild variant="outline" size="lg" disabled={page <= 1}>
                        <Link to="/products" search={{ page: page - 1 }} disabled={page <= 1}>
                            <ChevronLeft /> {t("catalog.previous")}
                        </Link>
                    </Button>
                    <span className="text-sm text-muted-foreground">{t("catalog.pageOf", { page, pages: pageCount })}</span>
                    <Button asChild variant="outline" size="lg" disabled={page >= pageCount}>
                        <Link to="/products" search={{ page: page + 1 }} disabled={page >= pageCount}>
                            {t("catalog.next")} <ChevronRight />
                        </Link>
                    </Button>
                </nav>
            )}
        </div>
    );
}
