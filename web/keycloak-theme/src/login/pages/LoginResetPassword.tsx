import { kcSanitize } from "keycloakify/lib/kcSanitize";
import type { PageProps } from "keycloakify/login/pages/PageProps";
import { Button } from "@/components/ui/button";
import { Field, FieldError, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import type { I18n } from "../i18n";
import type { KcContext } from "../KcContext";
import Template from "../Template";

export default function LoginResetPassword(
    props: PageProps<Extract<KcContext, { pageId: "login-reset-password.ftl" }>, I18n>
) {
    const { kcContext, i18n, doUseDefaultCss, classes } = props;
    const { url, realm, auth, messagesPerField } = kcContext;
    const { msg, msgStr } = i18n;

    const hasError = messagesPerField.existsError("username");

    return (
        <Template
            kcContext={kcContext}
            i18n={i18n}
            doUseDefaultCss={doUseDefaultCss}
            classes={classes}
            displayMessage={!hasError}
            headerNode={msg("emailForgotTitle")}
            subtitleNode={realm.duplicateEmailsAllowed ? msg("emailInstructionUsername") : msg("emailInstruction")}
        >
            <form id="kc-reset-password-form" action={url.loginAction} method="post">
                <FieldGroup>
                    <Field data-invalid={hasError}>
                        <FieldLabel htmlFor="username">
                            {realm.registrationEmailAsUsername ? msg("email") : msg("usernameOrEmail")}
                        </FieldLabel>
                        <Input
                            id="username"
                            name="username"
                            type={realm.registrationEmailAsUsername ? "email" : "text"}
                            placeholder={realm.registrationEmailAsUsername ? msgStr("emailPlaceholder") : undefined}
                            defaultValue={auth.attemptedUsername ?? ""}
                            autoComplete="username"
                            autoFocus
                            aria-invalid={hasError}
                        />
                        {hasError && (
                            <FieldError>
                                <span dangerouslySetInnerHTML={{ __html: kcSanitize(messagesPerField.get("username")) }} />
                            </FieldError>
                        )}
                    </Field>
                    <Field>
                        <Button type="submit" size="lg">
                            {msgStr("sendResetLink")}
                        </Button>
                        <p className="text-center text-sm">
                            <a href={url.loginUrl} className="font-medium text-primary underline-offset-4 hover:underline">
                                {msg("backToLogin")}
                            </a>
                        </p>
                    </Field>
                </FieldGroup>
            </form>
        </Template>
    );
}
