import { ShoppingBag } from "lucide-react";
import { useAppDispatch, useAppSelector } from "@/app/hooks";
import { Button } from "@/components/ui/button";
import { itemAdded } from "@/features/cart/cart-slice";
import { useTranslation } from "@/i18n/use-translation";
import type { StockState } from "../use-stock-levels";

export function AddToCartButton({ productId, stock, size = "default", className }: {
    productId: string;
    stock: StockState;
    size?: "default" | "lg";
    className?: string;
}) {
    const { t } = useTranslation();
    const dispatch = useAppDispatch();
    const inCart = useAppSelector(state => state.cart.items.find(item => item.productId === productId)?.quantity ?? 0);

    const available = stock.status === "known" ? stock.available : 0;
    const soldOut = stock.status === "known" && available === 0;
    const allInCart = available > 0 && inCart >= available;

    return (
        <Button
            size={size}
            className={className}
            disabled={stock.status !== "known" || soldOut || allInCart}
            onClick={() => dispatch(itemAdded({ productId }))}
        >
            <ShoppingBag />
            {soldOut ? t("stock.out") : allInCart ? t("product.allInCart") : t("product.addToCart")}
            {inCart > 0 && !allInCart && <span className="rounded-full bg-white/20 px-1.5 text-xs">{inCart}</span>}
        </Button>
    );
}
