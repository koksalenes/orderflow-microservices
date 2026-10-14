import { queryOptions } from "@tanstack/react-query";
import { http } from "@/lib/http";
import { ApiError } from "@/lib/problem-details";

export type Session = {
    subject: string;
    name: string;
    givenName?: string;
    familyName?: string;
    email?: string;
    emailVerified: boolean;
    roles: string[];
    logoutUrl: string;
    expiresAt?: string;
};

async function fetchSession(): Promise<Session | null> {
    try {
        const { data } = await http.get<Session>("/bff/user");
        return data;
    } catch (error) {
        if (error instanceof ApiError && error.status === 401) {
            return null;
        }
        throw error;
    }
}

export const sessionQueryOptions = queryOptions({
    queryKey: ["session"],
    queryFn: fetchSession,
    staleTime: 5 * 60_000
});

export function signIn(options: { returnUrl?: string; register?: boolean; locale?: string } = {}) {
    const params = new URLSearchParams({ returnUrl: options.returnUrl ?? window.location.pathname + window.location.search });
    if (options.register) {
        params.set("prompt", "create");
    }
    if (options.locale) {
        params.set("locale", options.locale);
    }
    window.location.assign(`/bff/login?${params}`);
}

export function signOut(session: Session) {
    window.location.assign(session.logoutUrl);
}
