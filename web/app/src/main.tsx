import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { Provider as ReduxProvider } from "react-redux";
import { QueryClientProvider } from "@tanstack/react-query";
import { RouterProvider, createRouter } from "@tanstack/react-router";
import { queryClient } from "./app/query-client";
import { store } from "./app/store";
import { sessionQueryOptions } from "./features/auth/session";
import { onUnauthorized } from "./lib/http";
import { routeTree } from "./routeTree.gen";
import "./index.css";

const router = createRouter({
    routeTree,
    context: { queryClient, store },
    defaultPreload: "intent",
    defaultPreloadStaleTime: 0,
    scrollRestoration: true
});

declare module "@tanstack/react-router" {
    interface Register {
        router: typeof router;
    }
}

/** A 401 from any API call means the server-side session is gone: refresh what the UI knows about it. */
onUnauthorized(() => void queryClient.invalidateQueries({ queryKey: sessionQueryOptions.queryKey }));

document.documentElement.lang = store.getState().preferences.locale;

createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <ReduxProvider store={store}>
            <QueryClientProvider client={queryClient}>
                <RouterProvider router={router} />
            </QueryClientProvider>
        </ReduxProvider>
    </StrictMode>
);
