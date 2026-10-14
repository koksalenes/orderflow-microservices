import { useQuery } from "@tanstack/react-query";
import { stockLevelsQueryOptions } from "./catalog-api";

export type StockState = { status: "loading" } | { status: "unknown" } | { status: "known"; available: number };

export function useStockLevels(productIds: string[]) {
    const { data, isPending, isError } = useQuery(stockLevelsQueryOptions(productIds));

    return (productId: string): StockState => {
        if (isError) {
            return { status: "unknown" };
        }
        if (isPending) {
            return { status: "loading" };
        }
        const available = data[productId];
        return available === undefined ? { status: "unknown" } : { status: "known", available };
    };
}
