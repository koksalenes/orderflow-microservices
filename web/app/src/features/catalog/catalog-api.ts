import { keepPreviousData, queryOptions } from "@tanstack/react-query";
import { http } from "@/lib/http";

export type ProductSummary = {
    id: string;
    name: string;
    category: string;
    price: number;
    currency: string;
    imageUrl: string | null;
};

export type ProductSpecification = { name: string; value: string };

export type Product = {
    id: string;
    name: string;
    description: string;
    category: string;
    price: number;
    currency: string;
    images: string[];
    specifications: ProductSpecification[];
};

export type Page<T> = { items: T[]; page: number; pageSize: number; totalCount: number };

type StockLevel = { productId: string; available: number };

export type StockLevels = Record<string, number>;

export const catalogPageSize = 12;
export const featuredCount = 8;

export const maxSearchLength = 100;

export const productsQueryOptions = (page: number, pageSize = catalogPageSize, q = "") =>
    queryOptions({
        queryKey: ["products", "list", { page, pageSize, q }],
        queryFn: async ({ signal }) =>
            (await http.get<Page<ProductSummary>>("/api/v1/products", { params: { page, pageSize, q: q || undefined }, signal })).data,
        placeholderData: keepPreviousData,
        staleTime: 5 * 60_000
    });

export const productQueryOptions = (productId: string) =>
    queryOptions({
        queryKey: ["products", "detail", productId],
        queryFn: async ({ signal }) => (await http.get<Product>(`/api/v1/products/${productId}`, { signal })).data,
        staleTime: 5 * 60_000
    });

export const stockLevelsQueryOptions = (productIds: string[]) =>
    queryOptions({
        queryKey: ["stock", [...productIds].sort()],
        queryFn: async ({ signal }) => {
            const params = new URLSearchParams();
            productIds.forEach(id => params.append("productId", id));
            const { data } = await http.get<StockLevel[]>("/api/v1/inventory/stock", { params, signal });
            return Object.fromEntries(data.map(level => [level.productId, level.available])) as StockLevels;
        },
        enabled: productIds.length > 0,
        staleTime: 15_000
    });
