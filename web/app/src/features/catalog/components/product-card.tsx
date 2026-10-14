import { Link } from "@tanstack/react-router";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { useTranslation } from "@/i18n/use-translation";
import { formatPrice } from "@/lib/format";
import type { ProductSummary } from "../catalog-api";
import type { StockState } from "../use-stock-levels";
import { AddToCartButton } from "./add-to-cart-button";
import { ProductImage } from "./product-image";
import { StockBadge } from "./stock-badge";

export function ProductCard({ product, stock }: { product: ProductSummary; stock: StockState }) {
    const { locale } = useTranslation();

    return (
        <Card className="group gap-0 overflow-hidden py-0 transition-shadow hover:shadow-md">
            <Link to="/products/$productId" params={{ productId: product.id }} className="flex flex-1 flex-col">
                <ProductImage src={product.imageUrl} alt={product.name} className="border-b" />
                <CardContent className="flex flex-1 flex-col gap-1.5 p-4">
                    <p className="text-xs font-medium tracking-wide text-muted-foreground uppercase">{product.category}</p>
                    <p className="line-clamp-2 font-medium group-hover:text-primary">{product.name}</p>
                    <p className="mt-auto pt-1 text-lg font-semibold">{formatPrice(product.price, product.currency, locale)}</p>
                    <StockBadge stock={stock} />
                </CardContent>
            </Link>
            <div className="px-4 pb-4">
                <AddToCartButton productId={product.id} stock={stock} className="w-full" />
            </div>
        </Card>
    );
}

export function ProductCardSkeleton() {
    return (
        <Card className="gap-0 py-0">
            <Skeleton className="aspect-square rounded-none" />
            <CardContent className="flex flex-col gap-2 p-4">
                <Skeleton className="h-3 w-1/3" />
                <Skeleton className="h-4 w-3/4" />
                <Skeleton className="h-5 w-1/4" />
                <Skeleton className="h-8 w-full" />
            </CardContent>
        </Card>
    );
}
