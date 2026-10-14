import { useCallback } from "react";
import { useAppSelector } from "@/app/hooks";
import { messages, type MessageKey } from "./messages";

export function useTranslation() {
    const locale = useAppSelector(state => state.preferences.locale);
    const t = useCallback((key: MessageKey) => messages[locale][key], [locale]);

    return { t, locale };
}
