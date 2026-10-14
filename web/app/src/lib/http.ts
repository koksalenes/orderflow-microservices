import axios, { AxiosError } from "axios";
import { ApiError, type ProblemDetails } from "./problem-details";

export const http = axios.create({
    baseURL: "/",
    timeout: 15_000,
    withCredentials: true,
    headers: { "X-CSRF": "1", Accept: "application/json" }
});

type UnauthorizedListener = () => void;
const unauthorizedListeners = new Set<UnauthorizedListener>();

export function onUnauthorized(listener: UnauthorizedListener) {
    unauthorizedListeners.add(listener);
    return () => unauthorizedListeners.delete(listener);
}

http.interceptors.response.use(
    response => response,
    (error: unknown) => {
        if (!(error instanceof AxiosError) || error.response === undefined) {
            return Promise.reject(error);
        }

        const { status, data } = error.response;
        if (status === 401) {
            unauthorizedListeners.forEach(listener => listener());
        }

        const problem: ProblemDetails = typeof data === "object" && data !== null ? data : { status };
        return Promise.reject(new ApiError(status, problem));
    }
);
