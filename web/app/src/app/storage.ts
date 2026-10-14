const prefix = "orderflow.";

export function readStorage<T>(key: string): T | undefined {
    try {
        const raw = localStorage.getItem(prefix + key);
        return raw === null ? undefined : (JSON.parse(raw) as T);
    } catch {
        return undefined;
    }
}

export function writeStorage(key: string, value: unknown) {
    try {
        localStorage.setItem(prefix + key, JSON.stringify(value));
    } catch {
        /** Ignored on purpose: persistence is a convenience only. */
    }
}
