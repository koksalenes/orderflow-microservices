import { useLayoutEffect, useState } from "react";
import type { JSX } from "keycloakify/tools/JSX";
import type { LazyOrNot } from "keycloakify/tools/LazyOrNot";
import { getKcClsx } from "keycloakify/login/lib/kcClsx";
import type { PageProps } from "keycloakify/login/pages/PageProps";
import type { UserProfileFormFieldsProps } from "keycloakify/login/UserProfileFormFieldsProps";
import { Button } from "@/components/ui/button";
import { Field, FieldDescription, FieldGroup, FieldLabel } from "@/components/ui/field";
import type { I18n } from "../i18n";
import type { KcContext } from "../KcContext";
import Template from "../Template";

type RegisterProps = PageProps<Extract<KcContext, { pageId: "register.ftl" }>, I18n> & {
    UserProfileFormFields: LazyOrNot<(props: UserProfileFormFieldsProps) => JSX.Element>;
    doMakeUserConfirmPassword: boolean;
};

export default function Register(props: RegisterProps) {
    const { kcContext, i18n, doUseDefaultCss, classes, UserProfileFormFields, doMakeUserConfirmPassword } = props;
    const { kcClsx } = getKcClsx({ doUseDefaultCss, classes });
    const {
        url,
        messagesPerField,
        recaptchaRequired,
        recaptchaVisible,
        recaptchaSiteKey,
        recaptchaAction,
        termsAcceptanceRequired
    } = kcContext;
    const { msg, msgStr } = i18n;

    const [isFormSubmittable, setIsFormSubmittable] = useState(false);
    const [areTermsAccepted, setAreTermsAccepted] = useState(false);

    useLayoutEffect(() => {
        const callbacks = window as unknown as Record<string, unknown>;
        callbacks.onSubmitRecaptcha = () =>
            (document.getElementById("kc-register-form") as HTMLFormElement | null)?.requestSubmit();
        return () => {
            delete callbacks.onSubmitRecaptcha;
        };
    }, []);

    const isInvisibleRecaptcha = recaptchaRequired && !recaptchaVisible && recaptchaAction !== undefined;

    return (
        <Template
            kcContext={kcContext}
            i18n={i18n}
            doUseDefaultCss={doUseDefaultCss}
            classes={classes}
            headerNode={msg("registerTitle")}
            subtitleNode={msg("registerSubtitle")}
            displayMessage={messagesPerField.exists("global")}
            displayInfo
            infoNode={
                <>
                    {msg("alreadyHaveAccount")} <a href={url.loginUrl}>{msg("doLogIn")}</a>
                </>
            }
        >
            <form id="kc-register-form" action={url.registrationAction} method="post">
                <FieldGroup>
                    <div className="grid grid-cols-2 gap-x-3 gap-y-5 *:col-span-2 [&>*:has(#firstName)]:col-span-1 [&>*:has(#lastName)]:col-span-1">
                        <UserProfileFormFields
                            kcContext={kcContext}
                            i18n={i18n}
                            kcClsx={kcClsx}
                            onIsFormSubmittableValueChange={setIsFormSubmittable}
                            doMakeUserConfirmPassword={doMakeUserConfirmPassword}
                        />
                    </div>

                    {termsAcceptanceRequired && (
                        <Field orientation="horizontal" data-invalid={messagesPerField.existsError("termsAccepted")}>
                            <input
                                id="termsAccepted"
                                name="termsAccepted"
                                type="checkbox"
                                checked={areTermsAccepted}
                                onChange={event => setAreTermsAccepted(event.target.checked)}
                                className="size-4 accent-primary"
                            />
                            <FieldLabel htmlFor="termsAccepted" className="font-normal">
                                {msg("acceptTerms")}
                            </FieldLabel>
                        </Field>
                    )}

                    {recaptchaRequired && !isInvisibleRecaptcha && (
                        <div className="flex justify-center">
                            <div className="g-recaptcha" data-sitekey={recaptchaSiteKey} data-action={recaptchaAction} />
                        </div>
                    )}

                    <Field>
                        {isInvisibleRecaptcha ? (
                            <Button
                                type="submit"
                                size="lg"
                                className="g-recaptcha"
                                data-sitekey={recaptchaSiteKey}
                                data-callback="onSubmitRecaptcha"
                                data-action={recaptchaAction}
                            >
                                {msg("doRegister")}
                            </Button>
                        ) : (
                            <Button
                                type="submit"
                                size="lg"
                                disabled={!isFormSubmittable || (termsAcceptanceRequired && !areTermsAccepted)}
                            >
                                {msgStr("doRegister")}
                            </Button>
                        )}
                        <FieldDescription className="text-center">{msg("registerHint")}</FieldDescription>
                    </Field>
                </FieldGroup>
            </form>
        </Template>
    );
}
