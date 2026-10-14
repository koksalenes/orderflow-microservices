import { useNavigate } from "@tanstack/react-router";
import { Search, X } from "lucide-react";
import { useEffect, useRef, useState, type FormEvent } from "react";
import { Input } from "@/components/ui/input";
import { useTranslation } from "@/i18n/use-translation";
import { cn } from "@/lib/utils";
import { maxSearchLength } from "../catalog-api";

const debounceMs = 300;

export function ProductSearchForm({
    initialQuery = "",
    live = false,
    className,
    inputClassName,
    onSearched
}: {
    initialQuery?: string;
    live?: boolean;
    className?: string;
    inputClassName?: string;
    onSearched?: () => void;
}) {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const [value, setValue] = useState(initialQuery);
    const lastSent = useRef(initialQuery);

    useEffect(() => {
        if (initialQuery !== lastSent.current) {
            setValue(initialQuery);
            lastSent.current = initialQuery;
        }
    }, [initialQuery]);

    function search(query: string, replace: boolean) {
        const q = query.trim();
        lastSent.current = q;
        void navigate({ to: "/products", search: q.length > 0 ? { page: 1, q } : { page: 1 }, replace });
        onSearched?.();
    }

    useEffect(() => {
        if (!live || value.trim() === lastSent.current) {
            return;
        }
        const timer = window.setTimeout(() => search(value, true), debounceMs);
        return () => window.clearTimeout(timer);
    }, [value, live]);

    function submit(event: FormEvent) {
        event.preventDefault();
        search(value, false);
    }

    return (
        <form role="search" onSubmit={submit} className={cn("relative", className)}>
            <Search className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
            <Input
                type="search"
                value={value}
                maxLength={maxSearchLength}
                onChange={event => setValue(event.target.value)}
                placeholder={t("nav.search")}
                aria-label={t("nav.search")}
                className={cn("rounded-full bg-muted/60 pr-9 pl-9 hover:bg-muted [&::-webkit-search-cancel-button]:hidden", inputClassName)}
            />
            {value.length > 0 && (
                <button
                    type="button"
                    aria-label={t("catalog.clearSearch")}
                    onClick={() => {
                        setValue("");
                        if (live) search("", true);
                    }}
                    className="absolute top-1/2 right-2.5 flex size-5 -translate-y-1/2 cursor-pointer items-center justify-center rounded-full text-muted-foreground hover:bg-muted-foreground/15 hover:text-foreground"
                >
                    <X className="size-3.5" />
                </button>
            )}
        </form>
    );
}
