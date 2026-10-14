import { useQuery } from "@tanstack/react-query";
import { createFileRoute, Link } from "@tanstack/react-router";
import { ChevronLeft, ChevronRight, SearchX } from "lucide-react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { catalogPageSize, maxSearchLength, productsQueryOptions } from "@/features/catalog/catalog-api";
import { ProductSearchForm } from "@/features/catalog/components/product-search-form";
import { ProductGrid } from "@/features/catalog/components/product-grid";
import { useTranslation } from "@/i18n/use-translation";

type CatalogSearch = { page: number; q?: string };

export const Route = createFileRoute("/products/")({
    validateSearch: (search: Record<string, unknown>): CatalogSearch => {
        const page = Number(search.page);
        const q = typeof search.q === "string" ? search.q.trim().slice(0, maxSearchLength) : "";
        return { page: Number.isInteger(page) && page > 0 ? page : 1, ...(q.length > 0 ? { q } : {}) };
    },
    loaderDeps: ({ search }) => ({ page: search.page, q: search.q ?? "" }),
    loader: ({ context, deps }) => void context.queryClient.prefetchQuery(productsQueryOptions(deps.page, catalogPageSize, deps.q)),
    component: CatalogPage
});

function CatalogPage() {
    const { page, q = "" } = Route.useSearch();
    const { t } = useTranslation();
    const { data, isError, isPlaceholderData } = useQuery(productsQueryOptions(page, catalogPageSize, q));

    const pageCount = data === undefined ? 1 : Math.max(1, Math.ceil(data.totalCount / data.pageSize));

    return (
        <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
            <div className="mb-8 flex flex-wrap items-end justify-between gap-4">
                <div>
                    <h1 className="text-3xl font-semibold tracking-tight">{t("catalog.title")}</h1>
                    <p className="mt-1 text-sm text-muted-foreground">
                        {data === undefined
                            ? t("catalog.subtitle")
                            : q.length > 0
                              ? t("catalog.resultsFor", { count: data.totalCount, q })
                              : t("catalog.count", { count: data.totalCount })}
                    </p>
                </div>
                <ProductSearchForm initialQuery={q} live className="w-full sm:w-80" inputClassName="h-10" />
            </div>

            {isError ? (
                <Alert variant="destructive">
                    <AlertDescription>{t("catalog.loadError")}</AlertDescription>
                </Alert>
            ) : data !== undefined && data.totalCount === 0 && q.length > 0 ? (
                <div className="flex flex-col items-center gap-3 rounded-2xl border border-dashed px-6 py-16 text-center">
                    <SearchX className="size-10 text-muted-foreground/60" />
                    <p className="font-medium">{t("catalog.noResults", { q })}</p>
                    <p className="text-sm text-muted-foreground">{t("catalog.noResultsHint")}</p>
                    <Button asChild variant="outline">
                        <Link to="/products" search={{ page: 1 }}>
                            {t("catalog.clearSearch")}
                        </Link>
                    </Button>
                </div>
            ) : (
                <div className={isPlaceholderData ? "opacity-60 transition-opacity" : "transition-opacity"}>
                    <ProductGrid products={data?.items} skeletons={catalogPageSize} />
                </div>
            )}

            {data !== undefined && pageCount > 1 && (
                <nav className="mt-10 flex items-center justify-center gap-3" aria-label={t("catalog.pagination")}>
                    <Button asChild variant="outline" size="lg" disabled={page <= 1}>
                        <Link to="/products" search={{ page: page - 1, ...(q ? { q } : {}) }} disabled={page <= 1}>
                            <ChevronLeft /> {t("catalog.previous")}
                        </Link>
                    </Button>
                    <span className="text-sm text-muted-foreground">{t("catalog.pageOf", { page, pages: pageCount })}</span>
                    <Button asChild variant="outline" size="lg" disabled={page >= pageCount}>
                        <Link to="/products" search={{ page: page + 1, ...(q ? { q } : {}) }} disabled={page >= pageCount}>
                            {t("catalog.next")} <ChevronRight />
                        </Link>
                    </Button>
                </nav>
            )}
        </div>
    );
}
