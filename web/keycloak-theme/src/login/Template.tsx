import { useEffect, type ReactNode } from "react";
import { AlertCircle, Bell, CheckCircle2, Info, PackageSearch, RotateCcw, ShieldCheck, TriangleAlert, Workflow } from "lucide-react";
import { kcSanitize } from "keycloakify/lib/kcSanitize";
import type { TemplateProps } from "keycloakify/login/TemplateProps";
import { useInitialize } from "keycloakify/login/Template.useInitialize";
import { cn } from "cn";
import { Alert, AlertDescription } from "@/components/ui/alert";
import type { I18n } from "./i18n";
import type { KcContext } from "./KcContext";

type Props = TemplateProps<KcContext, I18n> & { subtitleNode?: ReactNode };

const messageIcons = {
    success: CheckCircle2,
    warning: TriangleAlert,
    error: AlertCircle,
    info: Info
} as const;

export default function Template(props: Props) {
    const {
        displayInfo = false,
        displayMessage = true,
        displayRequiredFields = false,
        headerNode,
        subtitleNode,
        infoNode = null,
        documentTitle,
        kcContext,
        i18n,
        doUseDefaultCss,
        children
    } = props;

    const { msg, msgStr, currentLanguage, enabledLanguages } = i18n;
    const { realm, auth, url, message, isAppInitiatedAction } = kcContext;

    useEffect(() => {
        document.title = documentTitle ?? msgStr("loginTitle", realm.displayName || realm.name);
    }, []);

    const { isReadyToRender } = useInitialize({ kcContext, doUseDefaultCss });

    if (!isReadyToRender) {
        return null;
    }

    const brandName = realm.displayName || "OrderFlow";
    const showAttemptedUsername = auth !== undefined && auth.showUsername && !auth.showResetCredentials;
    const showMessage =
        displayMessage && message !== undefined && (message.type !== "warning" || !isAppInitiatedAction);
    const MessageIcon = message !== undefined ? messageIcons[message.type] : Info;

    const features = [
        { icon: PackageSearch, title: msg("brandFeatureTrackingTitle"), text: msg("brandFeatureTrackingText") },
        { icon: ShieldCheck, title: msg("brandFeatureCheckoutTitle"), text: msg("brandFeatureCheckoutText") },
        { icon: Bell, title: msg("brandFeatureNotificationsTitle"), text: msg("brandFeatureNotificationsText") }
    ];

    return (
        <div className="grid min-h-svh bg-background lg:grid-cols-[minmax(0,5fr)_minmax(0,6fr)]">
            <aside className="relative hidden overflow-hidden bg-linear-to-br from-blue-700 via-blue-600 to-sky-500 p-12 text-white lg:flex lg:flex-col lg:justify-between">
                <div aria-hidden className="pointer-events-none absolute -top-24 -right-24 size-96 rounded-full bg-white/10 blur-2xl" />
                <div aria-hidden className="pointer-events-none absolute -bottom-32 -left-16 size-112 rounded-full bg-sky-300/20 blur-3xl" />

                <Brand name={brandName} href={url.loginUrl} inverted />

                <div className="relative max-w-md">
                    <h2 className="text-4xl leading-tight font-semibold tracking-tight text-balance">{msg("brandHeadline")}</h2>
                    <p className="mt-4 text-base leading-relaxed text-blue-50/90">{msg("brandDescription")}</p>
                    <ul className="mt-10 flex flex-col gap-6">
                        {features.map(({ icon: Icon, title, text }, index) => (
                            <li key={index} className="flex gap-4">
                                <span className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-white/15 ring-1 ring-white/25 backdrop-blur-sm">
                                    <Icon className="size-5" />
                                </span>
                                <div>
                                    <p className="font-medium">{title}</p>
                                    <p className="text-sm leading-relaxed text-blue-50/80">{text}</p>
                                </div>
                            </li>
                        ))}
                    </ul>
                </div>

                <p className="relative text-sm text-blue-50/70">{msg("copyright", String(new Date().getFullYear()))}</p>
            </aside>

            <main className="flex flex-col bg-muted/40 lg:bg-background">
                <header className="flex items-center justify-between gap-4 p-6 md:px-10">
                    <div className="lg:invisible">
                        <Brand name={brandName} href={url.loginUrl} />
                    </div>
                    {enabledLanguages.length > 1 && (
                        <nav
                            aria-label={msgStr("languages")}
                            className="flex items-center gap-1 rounded-full border bg-background p-1 text-xs shadow-xs"
                        >
                            {enabledLanguages.map(({ languageTag, label, href }) => (
                                <a
                                    key={languageTag}
                                    href={href}
                                    aria-current={languageTag === currentLanguage.languageTag ? "true" : undefined}
                                    className={cn(
                                        "rounded-full px-3 py-1 font-medium transition-colors",
                                        languageTag === currentLanguage.languageTag
                                            ? "bg-primary text-primary-foreground"
                                            : "text-muted-foreground hover:bg-muted hover:text-foreground"
                                    )}
                                >
                                    {label}
                                </a>
                            ))}
                        </nav>
                    )}
                </header>

                <div className="flex flex-1 items-center justify-center px-6 pb-10 md:px-10">
                    <div className="w-full max-w-sm rounded-2xl border bg-card p-6 shadow-sm sm:p-8 lg:border-0 lg:bg-transparent lg:p-0 lg:shadow-none">
                        <div className="mb-7 flex flex-col gap-1.5">
                            {showAttemptedUsername ? (
                                <div className="flex items-center gap-2">
                                    <h1 className="text-xl font-semibold tracking-tight">{auth.attemptedUsername}</h1>
                                    <a
                                        href={url.loginRestartFlowUrl}
                                        aria-label={msgStr("restartLoginTooltip")}
                                        title={msgStr("restartLoginTooltip")}
                                        className="rounded-md p-1 text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
                                    >
                                        <RotateCcw className="size-4" />
                                    </a>
                                </div>
                            ) : (
                                <h1 className="text-2xl font-semibold tracking-tight">{headerNode}</h1>
                            )}
                            {subtitleNode !== undefined && <p className="text-sm text-muted-foreground">{subtitleNode}</p>}
                            {displayRequiredFields && (
                                <p className="text-sm text-muted-foreground">
                                    <span className="text-destructive">*</span> {msg("requiredFields")}
                                </p>
                            )}
                        </div>

                        <div className="flex flex-col gap-6">
                            {showMessage && (
                                <Alert
                                    variant={message.type === "error" ? "destructive" : "default"}
                                    className={cn(
                                        message.type === "error" && "border-destructive/30 bg-destructive/5",
                                        message.type === "success" && "border-emerald-200 bg-emerald-50 text-emerald-800",
                                        message.type === "warning" && "border-amber-200 bg-amber-50 text-amber-900",
                                        message.type === "info" && "border-blue-200 bg-blue-50 text-blue-900"
                                    )}
                                >
                                    <MessageIcon />
                                    <AlertDescription className="text-current">
                                        <span dangerouslySetInnerHTML={{ __html: kcSanitize(message.summary) }} />
                                    </AlertDescription>
                                </Alert>
                            )}

                            {children}

                            {auth !== undefined && auth.showTryAnotherWayLink && (
                                <form id="kc-select-try-another-way-form" action={url.loginAction} method="post">
                                    <input type="hidden" name="tryAnotherWay" value="on" />
                                    <button
                                        type="submit"
                                        className="w-full cursor-pointer text-center text-sm font-medium text-primary underline-offset-4 hover:underline"
                                    >
                                        {msg("doTryAnotherWay")}
                                    </button>
                                </form>
                            )}

                            {displayInfo && (
                                <p className="text-center text-sm text-muted-foreground [&_a]:font-medium [&_a]:text-primary [&_a]:underline-offset-4 [&_a:hover]:underline">
                                    {infoNode}
                                </p>
                            )}
                        </div>
                    </div>
                </div>

                <footer className="px-6 pb-6 text-center text-xs text-balance text-muted-foreground md:px-10">
                    {msg("legalNotice")}
                </footer>
            </main>
        </div>
    );
}

function Brand(props: { name: string; href: string; inverted?: boolean }) {
    const { name, href, inverted = false } = props;

    return (
        <a href={href} className="relative flex items-center gap-2.5 text-lg font-semibold tracking-tight">
            <span
                className={cn(
                    "flex size-9 items-center justify-center rounded-xl shadow-sm",
                    inverted ? "bg-white text-blue-600" : "bg-primary text-primary-foreground"
                )}
            >
                <Workflow className="size-5" />
            </span>
            {name}
        </a>
    );
}
