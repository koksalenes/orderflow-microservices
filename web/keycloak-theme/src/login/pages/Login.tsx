import { useState } from "react";
import { kcSanitize } from "keycloakify/lib/kcSanitize";
import type { PageProps } from "keycloakify/login/pages/PageProps";
import { Button } from "@/components/ui/button";
import { Field, FieldError, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { PasswordInput } from "../components/password-input";
import type { I18n } from "../i18n";
import type { KcContext } from "../KcContext";
import Template from "../Template";

export default function Login(props: PageProps<Extract<KcContext, { pageId: "login.ftl" }>, I18n>) {
    const { kcContext, i18n, doUseDefaultCss, classes } = props;
    const { realm, url, usernameHidden, login, auth, registrationDisabled, messagesPerField } = kcContext;
    const { msg, msgStr } = i18n;

    const [isSubmitting, setIsSubmitting] = useState(false);
    const hasError = messagesPerField.existsError("username", "password");
    const usernameLabel = !realm.loginWithEmailAllowed
        ? msg("username")
        : realm.registrationEmailAsUsername
          ? msg("email")
          : msg("usernameOrEmail");

    return (
        <Template
            kcContext={kcContext}
            i18n={i18n}
            doUseDefaultCss={doUseDefaultCss}
            classes={classes}
            displayMessage={!hasError}
            headerNode={msg("loginAccountTitle")}
            subtitleNode={msg("loginAccountSubtitle")}
            displayInfo={realm.password && realm.registrationAllowed && !registrationDisabled}
            infoNode={
                <>
                    {msg("noAccount")} <a href={url.registrationUrl}>{msg("doRegister")}</a>
                </>
            }
        >
            {realm.password && (
                <form
                    id="kc-form-login"
                    action={url.loginAction}
                    method="post"
                    onSubmit={() => {
                        setIsSubmitting(true);
                        return true;
                    }}
                >
                    <FieldGroup>
                        {!usernameHidden && (
                            <Field data-invalid={hasError}>
                                <FieldLabel htmlFor="username">{usernameLabel}</FieldLabel>
                                <Input
                                    id="username"
                                    name="username"
                                    type={realm.registrationEmailAsUsername ? "email" : "text"}
                                    placeholder={realm.registrationEmailAsUsername ? msgStr("emailPlaceholder") : undefined}
                                    defaultValue={login.username ?? ""}
                                    autoComplete="username"
                                    autoFocus
                                    aria-invalid={hasError}
                                />
                            </Field>
                        )}

                        <Field data-invalid={hasError}>
                            <div className="flex items-center">
                                <FieldLabel htmlFor="password">{msg("password")}</FieldLabel>
                                {realm.resetPasswordAllowed && (
                                    <a
                                        href={url.loginResetCredentialsUrl}
                                        className="ml-auto text-sm font-medium text-primary underline-offset-4 hover:underline"
                                    >
                                        {msg("doForgotPassword")}
                                    </a>
                                )}
                            </div>
                            <PasswordInput
                                id="password"
                                name="password"
                                autoComplete="current-password"
                                aria-invalid={hasError}
                                showLabel={msgStr("showPassword")}
                                hideLabel={msgStr("hidePassword")}
                            />
                            {hasError && (
                                <FieldError>
                                    <span
                                        dangerouslySetInnerHTML={{
                                            __html: kcSanitize(messagesPerField.getFirstError("username", "password"))
                                        }}
                                    />
                                </FieldError>
                            )}
                        </Field>

                        {realm.rememberMe && <input type="hidden" name="rememberMe" value="on" />}

                        <input type="hidden" name="credentialId" value={auth.selectedCredential ?? ""} />
                        <Button type="submit" name="login" size="lg" disabled={isSubmitting}>
                            {msgStr("doLogIn")}
                        </Button>
                    </FieldGroup>
                </form>
            )}
        </Template>
    );
}
