import { queryOptions } from "@tanstack/react-query";
import type { Page } from "@/features/catalog/catalog-api";
import { http } from "@/lib/http";

export type OrderStatus =
    | "Placed"
    | "AwaitingPayment"
    | "Paid"
    | "Confirmed"
    | "Shipped"
    | "Delivered"
    | "Completed"
    | "Cancelled"
    | "Refunding"
    | "Refunded";

export type PaymentMethod = "wallet" | "card";

export type ShippingAddress = {
    fullName: string;
    line1: string;
    line2?: string | null;
    city: string;
    postalCode: string;
    country: string;
    phone: string;
};

export type OrderLine = {
    productId: string;
    name: string;
    unitPrice: number;
    quantity: number;
    lineTotal: number;
    imageUrl: string | null;
};

export type OrderStep = { name: string; occurredAt: string; detail: string | null };

export type Order = {
    id: string;
    number: string;
    status: OrderStatus;
    cancellationReason: string | null;
    lines: OrderLine[];
    subtotal: number;
    shippingFee: number;
    total: number;
    currency: string;
    paymentMethod: PaymentMethod;
    cardBrand: string | null;
    cardLast4: string | null;
    shippingAddress: ShippingAddress;
    trackingNumber: string | null;
    invoiceNumber: string | null;
    scenario: string | null;
    placedAt: string;
    updatedAt: string;
    nextActionAt: string | null;
    steps: OrderStep[];
};

export type OrderSummary = {
    id: string;
    number: string;
    status: OrderStatus;
    cancellationReason: string | null;
    total: number;
    currency: string;
    paymentMethod: PaymentMethod;
    itemCount: number;
    images: string[];
    placedAt: string;
    updatedAt: string;
};

export type PlaceOrderRequest = {
    lines: { productId: string; quantity: number }[];
    shippingAddress: ShippingAddress;
    paymentMethod: PaymentMethod;
    cardPaymentId?: string;
    cardBrand?: string;
    cardLast4?: string;
    scenario?: "lost_parcel";
    locale: "en" | "tr";
};

export type CardPayment = {
    id: string;
    amount: number;
    currency: string;
    cardBrand: string;
    cardLast4: string;
    authorizationCode: string;
    status: string;
};

const finalStatuses: OrderStatus[] = ["Completed", "Cancelled", "Refunded"];

export const isOrderActive = (status: OrderStatus) => !finalStatuses.includes(status);

export const ordersKey = ["orders"] as const;

export const myOrdersQueryOptions = (page: number, pageSize = 10) =>
    queryOptions({
        queryKey: [...ordersKey, "mine", { page, pageSize }],
        queryFn: async ({ signal }) =>
            (await http.get<Page<OrderSummary>>("/api/v1/orders", { params: { page, pageSize }, signal })).data,
        refetchInterval: query => (query.state.data?.items.some(order => isOrderActive(order.status)) ? 3000 : false)
    });

export const orderQueryOptions = (orderId: string) =>
    queryOptions({
        queryKey: [...ordersKey, "detail", orderId],
        queryFn: async ({ signal }) => (await http.get<Order>(`/api/v1/orders/${orderId}`, { signal })).data,
        refetchInterval: query => (query.state.data !== undefined && isOrderActive(query.state.data.status) ? 1500 : false)
    });

export async function authorizeCardPayment(request: {
    amount: number;
    card: { holderName: string; number: string; expiryMonth: number; expiryYear: number; cvv: string };
}) {
    return (await http.post<CardPayment>("/api/v1/payments/card-payments", request)).data;
}

export async function placeOrder(request: PlaceOrderRequest) {
    return (await http.post<Order>("/api/v1/orders", request)).data;
}
