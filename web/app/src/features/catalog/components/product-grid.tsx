import type { ProductSummary } from "../catalog-api";
import { useStockLevels } from "../use-stock-levels";
import { ProductCard, ProductCardSkeleton } from "./product-card";

export function ProductGrid({ products, skeletons }: { products: ProductSummary[] | undefined; skeletons: number }) {
    const stockOf = useStockLevels(products?.map(product => product.id) ?? []);

    return (
        <div className="grid grid-cols-2 gap-4 md:grid-cols-3 lg:grid-cols-4">
            {products === undefined
                ? Array.from({ length: skeletons }, (_, index) => <ProductCardSkeleton key={index} />)
                : products.map(product => <ProductCard key={product.id} product={product} stock={stockOf(product.id)} />)}
        </div>
    );
}
