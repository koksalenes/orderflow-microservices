import { useCallback } from "react";
import { useAppSelector } from "@/app/hooks";
import { messages, type MessageKey } from "./messages";

export function useTranslation() {
    const locale = useAppSelector(state => state.preferences.locale);
    const t = useCallback(
        (key: MessageKey, params?: Record<string, string | number>) =>
            params === undefined
                ? messages[locale][key]
                : messages[locale][key].replace(/\{(\w+)\}/g, (match, name: string) => String(params[name] ?? match)),
        [locale]
    );

    return { t, locale };
}
