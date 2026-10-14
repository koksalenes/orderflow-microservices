import { useQuery } from "@tanstack/react-query";
import { sessionQueryOptions } from "./session";

export function useSession() {
    const { data, isPending } = useQuery(sessionQueryOptions);
    return { session: data ?? null, isPending };
}
