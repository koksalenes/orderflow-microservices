import { useSuspenseQuery } from "@tanstack/react-query";
import { createFileRoute, Link, notFound } from "@tanstack/react-router";
import { ArrowLeft } from "lucide-react";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { productQueryOptions } from "@/features/catalog/catalog-api";
import { AddToCartButton } from "@/features/catalog/components/add-to-cart-button";
import { ProductImage } from "@/features/catalog/components/product-image";
import { StockBadge } from "@/features/catalog/components/stock-badge";
import { useStockLevels } from "@/features/catalog/use-stock-levels";
import { useTranslation } from "@/i18n/use-translation";
import { formatPrice } from "@/lib/format";
import { ApiError } from "@/lib/problem-details";
import { cn } from "@/lib/utils";

export const Route = createFileRoute("/products/$productId")({
    loader: async ({ context, params }) => {
        try {
            await context.queryClient.ensureQueryData(productQueryOptions(params.productId));
        } catch (error) {
            /** Unknown id (404) or a malformed one (the route constraint makes the service answer 404 too). */
            if (error instanceof ApiError && error.status === 404) {
                throw notFound();
            }
            throw error;
        }
    },
    component: ProductPage,
    notFoundComponent: ProductNotFound
});

function ProductPage() {
    const { productId } = Route.useParams();
    const { t, locale } = useTranslation();
    const { data: product } = useSuspenseQuery(productQueryOptions(productId));
    const stock = useStockLevels([product.id])(product.id);
    const [selected, setSelected] = useState(0);

    const image = product.images[selected] ?? product.images[0] ?? null;

    return (
        <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
            <Button asChild variant="ghost" className="-ml-2.5 text-muted-foreground">
                <Link to="/products" search={{ page: 1 }}>
                    <ArrowLeft /> {t("product.backToCatalog")}
                </Link>
            </Button>

            <div className="mt-4 grid gap-10 lg:grid-cols-2">
                <div className="flex flex-col gap-3">
                    <ProductImage src={image} alt={product.name} eager className="rounded-2xl border" />
                    {product.images.length > 1 && (
                        <div className="grid grid-cols-4 gap-3">
                            {product.images.map((url, index) => (
                                <button
                                    key={url}
                                    type="button"
                                    aria-label={`${product.name} ${index + 1}`}
                                    aria-pressed={index === selected}
                                    onClick={() => setSelected(index)}
                                    className={cn(
                                        "cursor-pointer overflow-hidden rounded-xl border transition-shadow hover:shadow-md",
                                        index === selected && "ring-2 ring-primary"
                                    )}
                                >
                                    <ProductImage src={url} alt="" />
                                </button>
                            ))}
                        </div>
                    )}
                </div>

                <div className="flex flex-col gap-5">
                    <div>
                        <p className="text-sm font-medium tracking-wide text-primary uppercase">{product.category}</p>
                        <h1 className="mt-2 text-3xl font-semibold tracking-tight text-balance">{product.name}</h1>
                    </div>
                    <p className="text-3xl font-semibold">{formatPrice(product.price, product.currency, locale)}</p>
                    <StockBadge stock={stock} className="text-sm" />
                    <AddToCartButton productId={product.id} stock={stock} size="lg" className="w-full sm:w-fit sm:min-w-56" />

                    <Separator />

                    <section>
                        <h2 className="text-lg font-semibold">{t("product.description")}</h2>
                        <p className="mt-2 leading-relaxed text-muted-foreground">{product.description}</p>
                    </section>

                    {product.specifications.length > 0 && (
                        <section>
                            <h2 className="text-lg font-semibold">{t("product.specifications")}</h2>
                            <dl className="mt-3 divide-y rounded-xl border text-sm">
                                {product.specifications.map(spec => (
                                    <div key={spec.name} className="grid grid-cols-[9rem_1fr] gap-4 px-4 py-2.5">
                                        <dt className="text-muted-foreground">{spec.name}</dt>
                                        <dd className="font-medium">{spec.value}</dd>
                                    </div>
                                ))}
                            </dl>
                        </section>
                    )}
                </div>
            </div>
        </div>
    );
}

function ProductNotFound() {
    const { t } = useTranslation();

    return (
        <section className="mx-auto flex max-w-xl flex-col items-center gap-4 px-4 py-24 text-center">
            <p className="text-sm font-semibold text-primary">404</p>
            <h1 className="text-3xl font-semibold tracking-tight">{t("product.notFound.title")}</h1>
            <p className="text-muted-foreground">{t("product.notFound.text")}</p>
            <Button asChild size="lg">
                <Link to="/products" search={{ page: 1 }}>
                    {t("product.backToCatalog")}
                </Link>
            </Button>
        </section>
    );
}
