import { useQueries } from "@tanstack/react-query";
import { useAppSelector } from "@/app/hooks";
import { productQueryOptions } from "@/features/catalog/catalog-api";
import { useStockLevels, type StockState } from "@/features/catalog/use-stock-levels";

export type CartLine = {
    productId: string;
    quantity: number;
    name: string;
    category: string;
    price: number;
    currency: string;
    imageUrl: string | null;
    stock: StockState;
    lineTotal: number;
};

export function useCartLines() {
    const items = useAppSelector(state => state.cart.items);
    const products = useQueries({ queries: items.map(item => productQueryOptions(item.productId)) });
    const stockOf = useStockLevels(items.map(item => item.productId));

    const isPending = products.some(product => product.isPending);
    const lines: CartLine[] = items.flatMap((item, index) => {
        const product = products[index]?.data;
        if (product === undefined) {
            return [];
        }
        return [
            {
                productId: item.productId,
                quantity: item.quantity,
                name: product.name,
                category: product.category,
                price: product.price,
                currency: product.currency,
                imageUrl: product.images[0] ?? null,
                stock: stockOf(item.productId),
                lineTotal: Math.round(product.price * item.quantity * 100) / 100
            }
        ];
    });
    const missing = items.filter((_, index) => products[index]?.isError).map(item => item.productId);
    const subtotal = Math.round(lines.reduce((sum, line) => sum + line.lineTotal, 0) * 100) / 100;
    const currency = lines[0]?.currency ?? "USD";
    const shortages = lines.filter(line => line.stock.status === "known" && line.stock.available < line.quantity);

    return { items, lines, missing, subtotal, currency, shortages, isPending };
}
