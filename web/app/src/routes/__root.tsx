import { Link, Outlet, createRootRouteWithContext, type ErrorComponentProps } from "@tanstack/react-router";
import { lazy, Suspense } from "react";
import { SiteFooter } from "@/components/layout/site-footer";
import { SiteHeader } from "@/components/layout/site-header";
import { Button } from "@/components/ui/button";
import type { RouterContext } from "@/app/router-context";
import { useTranslation } from "@/i18n/use-translation";
import { ApiError } from "@/lib/problem-details";

const Devtools = import.meta.env.DEV
    ? lazy(async () => {
          const [{ TanStackRouterDevtools }, { ReactQueryDevtools }] = await Promise.all([
              import("@tanstack/react-router-devtools"),
              import("@tanstack/react-query-devtools")
          ]);
          return {
              default: () => (
                  <>
                      <TanStackRouterDevtools position="bottom-left" />
                      <ReactQueryDevtools buttonPosition="bottom-right" />
                  </>
              )
          };
      })
    : () => null;

export const Route = createRootRouteWithContext<RouterContext>()({
    component: RootLayout,
    notFoundComponent: NotFound,
    errorComponent: RootError
});

function RootLayout() {
    return (
        <div className="flex min-h-svh flex-col bg-background">
            <SiteHeader />
            <main className="flex-1">
                <Outlet />
            </main>
            <SiteFooter />
            <Suspense>
                <Devtools />
            </Suspense>
        </div>
    );
}

function NotFound() {
    const { t } = useTranslation();

    return (
        <section className="mx-auto flex max-w-xl flex-col items-center gap-4 px-4 py-24 text-center">
            <p className="text-sm font-semibold text-primary">404</p>
            <h1 className="text-3xl font-semibold tracking-tight">{t("notFound.title")}</h1>
            <p className="text-muted-foreground">{t("notFound.text")}</p>
            <Button asChild size="lg">
                <Link to="/">{t("notFound.back")}</Link>
            </Button>
        </section>
    );
}

function RootError({ error, reset }: ErrorComponentProps) {
    const { t } = useTranslation();
    const reference = error instanceof ApiError ? (error.problem.correlationId ?? error.problem.traceId) : undefined;

    return (
        <section className="mx-auto flex max-w-xl flex-col items-center gap-4 px-4 py-24 text-center">
            <h1 className="text-3xl font-semibold tracking-tight">{t("error.title")}</h1>
            <p className="text-muted-foreground">{t("error.text")}</p>
            {reference !== undefined && <code className="rounded-md bg-muted px-2 py-1 text-xs">{reference}</code>}
            <Button size="lg" onClick={reset}>
                {t("error.retry")}
            </Button>
        </section>
    );
}
