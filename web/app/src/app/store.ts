import { combineSlices, configureStore, createListenerMiddleware } from "@reduxjs/toolkit";
import { cartSlice } from "@/features/cart/cart-slice";
import { localeChanged, preferencesSlice } from "@/features/preferences/preferences-slice";
import { writeStorage } from "./storage";

const rootReducer = combineSlices(preferencesSlice, cartSlice);

const persistence = createListenerMiddleware();

persistence.startListening({
    actionCreator: localeChanged,
    effect: action => {
        writeStorage("preferences.locale", action.payload);
        document.documentElement.lang = action.payload;
    }
});

persistence.startListening({
    predicate: action => action.type.startsWith(`${cartSlice.name}/`),
    effect: (_, api) => writeStorage("cart.items", (api.getState() as RootState).cart.items)
});

export const store = configureStore({
    reducer: rootReducer,
    middleware: getDefaultMiddleware => getDefaultMiddleware().prepend(persistence.middleware)
});

export type RootState = ReturnType<typeof rootReducer>;
export type AppStore = typeof store;
export type AppDispatch = typeof store.dispatch;
