import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import { readStorage } from "@/app/storage";

export type CartItem = { productId: string; quantity: number };

type CartState = { items: CartItem[] };

export const maxQuantityPerLine = 10;

export const cartSlice = createSlice({
    name: "cart",
    initialState: (): CartState => ({ items: readStorage<CartItem[]>("cart.items") ?? [] }),
    reducers: {
        itemAdded(state, action: PayloadAction<{ productId: string; quantity?: number }>) {
            const { productId, quantity = 1 } = action.payload;
            const existing = state.items.find(item => item.productId === productId);
            if (existing) {
                existing.quantity = Math.min(maxQuantityPerLine, existing.quantity + quantity);
            } else {
                state.items.push({ productId, quantity: Math.min(maxQuantityPerLine, quantity) });
            }
        },
        quantitySet(state, action: PayloadAction<{ productId: string; quantity: number }>) {
            const { productId, quantity } = action.payload;
            const existing = state.items.find(item => item.productId === productId);
            if (existing) {
                existing.quantity = Math.max(1, Math.min(maxQuantityPerLine, quantity));
            }
        },
        itemRemoved(state, action: PayloadAction<string>) {
            state.items = state.items.filter(item => item.productId !== action.payload);
        },
        cleared(state) {
            state.items = [];
        }
    },
    selectors: {
        selectItemCount: state => state.items.reduce((total, item) => total + item.quantity, 0)
    }
});

export const { itemAdded, quantitySet, itemRemoved, cleared } = cartSlice.actions;
export const { selectItemCount } = cartSlice.selectors;
