import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useRef } from "react";
import { walletKey, walletSagasQueryOptions } from "./wallet-api";

export function useWalletLiveUpdates() {
    const queryClient = useQueryClient();
    const sagas = useQuery(walletSagasQueryOptions);
    const signature = sagas.data?.map(saga => `${saga.depositId}:${saga.state}:${saga.updatedAt}`).join("|");
    const previous = useRef(signature);

    useEffect(() => {
        if (signature !== undefined && previous.current !== undefined && signature !== previous.current) {
            void queryClient.invalidateQueries({
                queryKey: walletKey,
                predicate: query => query.queryKey[1] !== "sagas"
            });
        }
        previous.current = signature;
    }, [signature, queryClient]);

    return sagas;
}
