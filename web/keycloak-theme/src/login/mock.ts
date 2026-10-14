import { createGetKcContextMock } from "keycloakify/login/KcContext";
import { kcEnvDefaults, themeNames } from "../kc.gen";
import type { KcContextExtension, KcContextExtensionPerPage } from "./KcContext";

const kcContextExtension: KcContextExtension = {
    themeName: themeNames[0],
    properties: { ...kcEnvDefaults }
};

const kcContextExtensionPerPage: KcContextExtensionPerPage = {};

export const { getKcContextMock } = createGetKcContextMock({
    kcContextExtension,
    kcContextExtensionPerPage,
    overrides: {
        realm: { displayName: "OrderFlow", registrationEmailAsUsername: true },
        locale: {
            currentLanguageTag: "en",
            supported: [
                { languageTag: "en", label: "English", url: "?kc_locale=en" },
                { languageTag: "tr", label: "Türkçe", url: "?kc_locale=tr" }
            ]
        }
    },
    overridesPerPage: {
        "login.ftl": { realm: { rememberMe: true, resetPasswordAllowed: true, registrationAllowed: true } }
    }
});
