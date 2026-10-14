import { useQuery } from "@tanstack/react-query";
import { Link } from "@tanstack/react-router";
import { ShoppingBag } from "lucide-react";
import { useTranslation } from "@/i18n/use-translation";
import { formatPrice } from "@/lib/format";
import { cn } from "@/lib/utils";
import { featuredCount, productsQueryOptions, type ProductSummary } from "../catalog-api";
import { useStockLevels, type StockState } from "../use-stock-levels";
import { ProductImage } from "./product-image";
import { StockBadge } from "./stock-badge";

const tiles = [
    "top-10 left-0 w-44 -rotate-6 xl:left-6",
    "right-0 top-0 z-10 w-60 rotate-3 xl:right-6",
    "bottom-0 left-24 w-48 rotate-2 xl:left-36"
];

export function HeroShowcase() {
    const { data, isError } = useQuery(productsQueryOptions(1, featuredCount));
    const products = data?.items.filter(product => product.imageUrl !== null).slice(0, tiles.length);
    const stockOf = useStockLevels(products?.map(product => product.id) ?? []);

    return (
        <div aria-hidden={products === undefined} className="relative hidden h-96 lg:block">
            {tiles.map((placement, index) => {
                const product = products?.[index];
                return product === undefined ? (
                    <div key={placement} className={cn("absolute flex aspect-[4/5] items-center justify-center rounded-2xl bg-white/15 ring-1 ring-white/25", placement)}>
                        {(isError || data !== undefined) && <ShoppingBag className="size-10 text-white/60" />}
                    </div>
                ) : (
                    <ShowcaseTile key={product.id} product={product} stock={stockOf(product.id)} placement={placement} large={index === 1} />
                );
            })}
        </div>
    );
}

function ShowcaseTile({ product, stock, placement, large }: { product: ProductSummary; stock: StockState; placement: string; large: boolean }) {
    const { locale } = useTranslation();

    return (
        <Link
            to="/products/$productId"
            params={{ productId: product.id }}
            className={cn(
                "group absolute rounded-2xl bg-white p-2 text-slate-900 shadow-2xl shadow-blue-950/30 transition-transform duration-300 hover:z-20 hover:-translate-y-1 hover:rotate-0",
                placement
            )}
        >
            <ProductImage src={product.imageUrl} alt={product.name} eager className="rounded-xl bg-blue-50/60" />
            <div className="flex flex-col gap-1 px-1.5 pt-2 pb-1">
                <p className={cn("truncate font-medium", large ? "text-sm" : "text-xs")}>{product.name}</p>
                <div className="flex items-center justify-between gap-2">
                    <span className={cn("font-semibold text-blue-700", large ? "text-base" : "text-sm")}>
                        {formatPrice(product.price, product.currency, locale)}
                    </span>
                    {large && <StockBadge stock={stock} />}
                </div>
            </div>
        </Link>
    );
}
