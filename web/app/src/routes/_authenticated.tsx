import { createFileRoute, Outlet, redirect } from "@tanstack/react-router";
import { sessionQueryOptions } from "@/features/auth/session";

export const Route = createFileRoute("/_authenticated")({
    beforeLoad: async ({ context, location }) => {
        const session = await context.queryClient.ensureQueryData(sessionQueryOptions);
        if (session === null) {
            const locale = context.store.getState().preferences.locale;
            const params = new URLSearchParams({ returnUrl: location.href, locale });
            throw redirect({ href: `/bff/login?${params}`, reloadDocument: true });
        }

        return { session };
    },
    component: Outlet
});
