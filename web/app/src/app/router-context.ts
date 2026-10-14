import type { QueryClient } from "@tanstack/react-query";
import type { AppStore } from "./store";

export type RouterContext = {
    queryClient: QueryClient;
    store: AppStore;
};
