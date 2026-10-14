import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import { locales, type Locale } from "@/i18n/messages";
import { readStorage } from "@/app/storage";

type PreferencesState = { locale: Locale };

function initialLocale(): Locale {
    const stored = readStorage<Locale>("preferences.locale");
    if (stored !== undefined && locales.includes(stored)) {
        return stored;
    }

    return navigator.language.toLowerCase().startsWith("tr") ? "tr" : "en";
}

export const preferencesSlice = createSlice({
    name: "preferences",
    initialState: (): PreferencesState => ({ locale: initialLocale() }),
    reducers: {
        localeChanged(state, action: PayloadAction<Locale>) {
            state.locale = action.payload;
        }
    }
});

export const { localeChanged } = preferencesSlice.actions;
