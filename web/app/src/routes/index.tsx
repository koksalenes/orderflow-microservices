import { createFileRoute, Link } from "@tanstack/react-router";
import { ArrowRight, PackageSearch, RotateCcw, ShieldCheck } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { signIn } from "@/features/auth/session";
import { useSession } from "@/features/auth/use-session";
import { useTranslation } from "@/i18n/use-translation";

export const Route = createFileRoute("/")({
    component: HomePage
});

function HomePage() {
    const { t, locale } = useTranslation();
    const { session } = useSession();

    const perks = [
        { icon: PackageSearch, title: t("home.perks.tracking.title"), text: t("home.perks.tracking.text") },
        { icon: ShieldCheck, title: t("home.perks.secure.title"), text: t("home.perks.secure.text") },
        { icon: RotateCcw, title: t("home.perks.returns.title"), text: t("home.perks.returns.text") }
    ];

    return (
        <>
            <section className="mx-auto max-w-7xl px-4 pt-6 sm:px-6 lg:px-8">
                <div className="relative overflow-hidden rounded-3xl bg-linear-to-br from-blue-700 via-blue-600 to-sky-500 px-6 py-14 text-white sm:px-12 sm:py-20">
                    <div aria-hidden className="pointer-events-none absolute -top-24 -right-24 size-96 rounded-full bg-white/10 blur-2xl" />
                    <div aria-hidden className="pointer-events-none absolute -bottom-32 left-1/3 size-112 rounded-full bg-sky-300/25 blur-3xl" />
                    <div className="relative max-w-2xl">
                        <span className="inline-flex items-center rounded-full bg-white/15 px-3 py-1 text-xs font-medium ring-1 ring-white/25">
                            {t("home.hero.eyebrow")}
                        </span>
                        <h1 className="mt-5 text-4xl leading-tight font-semibold tracking-tight text-balance sm:text-5xl">
                            {t("home.hero.title")}
                        </h1>
                        <p className="mt-4 max-w-xl text-base leading-relaxed text-blue-50/90 sm:text-lg">{t("home.hero.text")}</p>
                        <div className="mt-8 flex flex-wrap gap-3">
                            <Button size="lg" className="bg-white text-blue-700 hover:bg-blue-50 active:bg-blue-100" disabled>
                                {t("home.hero.primary")} <ArrowRight />
                            </Button>
                            {session !== null ? (
                                <Button asChild size="lg" variant="outline" className="border-white/40 bg-white/10 text-white hover:bg-white/20 hover:text-white">
                                    <Link to="/account">{t("home.hero.secondary")}</Link>
                                </Button>
                            ) : (
                                <Button
                                    size="lg"
                                    variant="outline"
                                    className="border-white/40 bg-white/10 text-white hover:bg-white/20 hover:text-white"
                                    onClick={() => signIn({ register: true, locale })}
                                >
                                    {t("auth.createAccount")}
                                </Button>
                            )}
                        </div>
                    </div>
                </div>
            </section>

            <section className="mx-auto grid max-w-7xl gap-4 px-4 py-10 sm:grid-cols-3 sm:px-6 lg:px-8">
                {perks.map(({ icon: Icon, title, text }) => (
                    <div key={title} className="flex gap-4 rounded-2xl border bg-card p-5 transition-shadow hover:shadow-md">
                        <span className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
                            <Icon className="size-5" />
                        </span>
                        <div>
                            <p className="font-medium">{title}</p>
                            <p className="mt-0.5 text-sm text-muted-foreground">{text}</p>
                        </div>
                    </div>
                ))}
            </section>

            <section className="mx-auto max-w-7xl px-4 pb-16 sm:px-6 lg:px-8">
                <div className="mb-6 flex items-end justify-between gap-4">
                    <div>
                        <h2 className="text-2xl font-semibold tracking-tight">{t("home.featured.title")}</h2>
                        <p className="mt-1 text-sm text-muted-foreground">{t("home.featured.text")}</p>
                    </div>
                </div>
                <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
                    {Array.from({ length: 8 }, (_, index) => (
                        <Card key={index} className="gap-0 py-0">
                            <Skeleton className="aspect-square rounded-none" />
                            <CardContent className="flex flex-col gap-2 p-4">
                                <Skeleton className="h-4 w-3/4" />
                                <Skeleton className="h-4 w-1/3" />
                            </CardContent>
                        </Card>
                    ))}
                </div>
            </section>
        </>
    );
}
