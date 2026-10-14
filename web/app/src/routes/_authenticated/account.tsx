import { createFileRoute } from "@tanstack/react-router";
import { BadgeCheck, CircleAlert, LogOut, PackageOpen, ShieldCheck } from "lucide-react";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { signOut } from "@/features/auth/session";
import { useTranslation } from "@/i18n/use-translation";
import { initials } from "@/lib/utils";

export const Route = createFileRoute("/_authenticated/account")({
    component: AccountPage
});

function AccountPage() {
    const { session } = Route.useRouteContext();
    const { t } = useTranslation();

    return (
        <div className="mx-auto max-w-5xl px-4 py-10 sm:px-6 lg:px-8">
            <div className="flex flex-col gap-6 sm:flex-row sm:items-center sm:justify-between">
                <div className="flex items-center gap-4">
                    <Avatar className="size-14">
                        <AvatarFallback className="bg-primary text-lg font-semibold text-primary-foreground">{initials(session.name)}</AvatarFallback>
                    </Avatar>
                    <div>
                        <h1 className="text-2xl font-semibold tracking-tight">{t("account.title")}</h1>
                        <p className="text-sm text-muted-foreground">{t("account.subtitle")}</p>
                    </div>
                </div>
                <Button variant="outline" size="lg" onClick={() => signOut(session)}>
                    <LogOut /> {t("auth.signOut")}
                </Button>
            </div>

            <div className="mt-8 grid gap-6 md:grid-cols-[3fr_2fr]">
                <Card>
                    <CardHeader>
                        <CardTitle>{t("account.profile")}</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <dl className="divide-y text-sm">
                            <Row label={t("account.name")}>{session.name}</Row>
                            <Row label={t("account.email")}>
                                <span className="flex flex-wrap items-center gap-2">
                                    {session.email}
                                    {session.emailVerified ? (
                                        <Badge variant="secondary" className="gap-1 text-emerald-700">
                                            <BadgeCheck className="size-3" /> {t("account.emailVerified")}
                                        </Badge>
                                    ) : (
                                        <Badge variant="secondary" className="gap-1 text-amber-700">
                                            <CircleAlert className="size-3" /> {t("account.emailNotVerified")}
                                        </Badge>
                                    )}
                                </span>
                            </Row>
                            <Row label={t("account.roles")}>
                                <span className="flex flex-wrap gap-1.5">
                                    {session.roles.map(role => (
                                        <Badge key={role} variant="outline">
                                            {role}
                                        </Badge>
                                    ))}
                                </span>
                            </Row>
                        </dl>
                    </CardContent>
                </Card>

                <div className="flex flex-col gap-6">
                    <Card>
                        <CardHeader>
                            <CardTitle className="flex items-center gap-2">
                                <ShieldCheck className="size-4 text-primary" /> {t("account.session")}
                            </CardTitle>
                            <CardDescription>{t("account.sessionText")}</CardDescription>
                        </CardHeader>
                    </Card>
                    <Card>
                        <CardHeader>
                            <CardTitle className="flex items-center gap-2">
                                <PackageOpen className="size-4 text-primary" /> {t("account.orders")}
                            </CardTitle>
                            <CardDescription>{t("account.ordersEmpty")}</CardDescription>
                        </CardHeader>
                    </Card>
                </div>
            </div>
        </div>
    );
}

function Row({ label, children }: { label: string; children: React.ReactNode }) {
    return (
        <div className="grid gap-1 py-3 first:pt-0 last:pb-0 sm:grid-cols-[10rem_1fr] sm:gap-4">
            <dt className="text-muted-foreground">{label}</dt>
            <dd className="font-medium">{children}</dd>
        </div>
    );
}
