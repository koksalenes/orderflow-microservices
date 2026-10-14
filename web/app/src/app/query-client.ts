import { QueryClient } from "@tanstack/react-query";
import { ApiError } from "@/lib/problem-details";

export const queryClient = new QueryClient({
    defaultOptions: {
        queries: {
            staleTime: 30_000,
            refetchOnWindowFocus: true,
            retry: (failureCount, error) =>
                !(error instanceof ApiError && error.status < 500) && failureCount < 2
        }
    }
});
