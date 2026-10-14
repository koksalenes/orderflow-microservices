import type { MessageKey } from "@/i18n/messages";
import type { OrderStatus } from "./orders-api";

export const statusKeys: Record<OrderStatus, MessageKey> = {
    Placed: "orders.status.Placed",
    AwaitingPayment: "orders.status.AwaitingPayment",
    Paid: "orders.status.Paid",
    Confirmed: "orders.status.Confirmed",
    Shipped: "orders.status.Shipped",
    Delivered: "orders.status.Delivered",
    Completed: "orders.status.Completed",
    Cancelled: "orders.status.Cancelled",
    Refunding: "orders.status.Refunding",
    Refunded: "orders.status.Refunded"
};

export const statusTone: Record<OrderStatus, string> = {
    Placed: "bg-blue-50 text-blue-700 ring-blue-200",
    AwaitingPayment: "bg-blue-50 text-blue-700 ring-blue-200",
    Paid: "bg-blue-50 text-blue-700 ring-blue-200",
    Confirmed: "bg-indigo-50 text-indigo-700 ring-indigo-200",
    Shipped: "bg-sky-50 text-sky-700 ring-sky-200",
    Delivered: "bg-emerald-50 text-emerald-700 ring-emerald-200",
    Completed: "bg-emerald-50 text-emerald-700 ring-emerald-200",
    Cancelled: "bg-red-50 text-red-700 ring-red-200",
    Refunding: "bg-amber-50 text-amber-800 ring-amber-200",
    Refunded: "bg-amber-50 text-amber-800 ring-amber-200"
};

export const stepKeys: Record<string, MessageKey> = {
    placed: "orders.step.placed",
    stock_reserved: "orders.step.stock_reserved",
    stock_reservation_failed: "orders.step.stock_reservation_failed",
    payment_requested: "orders.step.payment_requested",
    payment_captured: "orders.step.payment_captured",
    payment_failed: "orders.step.payment_failed",
    stock_released: "orders.step.stock_released",
    authorization_voided: "orders.step.authorization_voided",
    confirmed: "orders.step.confirmed",
    shipped: "orders.step.shipped",
    stock_committed: "orders.step.stock_committed",
    delivered: "orders.step.delivered",
    delivery_failed: "orders.step.delivery_failed",
    refund_requested: "orders.step.refund_requested",
    refunded: "orders.step.refunded",
    invoiced: "orders.step.invoiced",
    cancelled: "orders.step.cancelled"
};

export const failureSteps = new Set(["stock_reservation_failed", "payment_failed", "delivery_failed", "cancelled"]);
export const compensationSteps = new Set(["stock_released", "authorization_voided", "refund_requested", "refunded"]);

const reasonKeys: Record<string, MessageKey> = {
    out_of_stock: "orders.reason.out_of_stock",
    "payments.insufficient_funds": "orders.reason.insufficient_funds",
    card_capture_declined: "orders.reason.card_declined",
    lost_parcel: "orders.reason.lost_parcel"
};

export const reasonKey = (reason: string | null): MessageKey => (reason !== null && reasonKeys[reason]) || "orders.reason.unknown";
