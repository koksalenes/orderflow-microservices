export type ProblemDetails = {
    type?: string;
    title?: string;
    status?: number;
    detail?: string;
    instance?: string;
    code?: string;
    correlationId?: string;
    traceId?: string;
    errors?: Record<string, string[]>;
};

export class ApiError extends Error {
    constructor(
        readonly status: number,
        readonly problem: ProblemDetails
    ) {
        super(problem.title ?? `Request failed with status ${status}`);
        this.name = "ApiError";
    }

    get code() {
        return this.problem.code ?? "common.unexpected";
    }
}
