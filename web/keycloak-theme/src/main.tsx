import { createRoot } from "react-dom/client";
import { StrictMode } from "react";
import { KcPage, type KcContext } from "./kc.gen";
import "./index.css";

if (import.meta.env.DEV && window.kcContext === undefined) {
    const { getKcContextMock } = await import("./login/mock");
    const pageId = (new URLSearchParams(window.location.search).get("page") ?? "login.ftl") as KcContext["pageId"];
    window.kcContext = getKcContextMock({ pageId, overrides: {} });
}

createRoot(document.getElementById("root")!).render(
    <StrictMode>
        {window.kcContext === undefined ? <h1>No Keycloak Context</h1> : <KcPage kcContext={window.kcContext} />}
    </StrictMode>
);
