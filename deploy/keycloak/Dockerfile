# syntax=docker/dockerfile:1

# Keycloak with the OrderFlow login theme (web/keycloak-theme, built with Keycloakify).
# The theme jar is platform-independent, so it is built once on the build machine.

FROM --platform=$BUILDPLATFORM node:24.15.0-bookworm-slim AS theme
RUN apt-get update \
    && apt-get install -y --no-install-recommends maven openjdk-17-jdk-headless \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /theme
COPY web/keycloak-theme/package.json web/keycloak-theme/package-lock.json ./
RUN --mount=type=cache,id=orderflow-npm,target=/root/.npm npm ci
COPY web/keycloak-theme/ ./
RUN --mount=type=cache,id=orderflow-m2,target=/root/.m2 npm run build-keycloak-theme

FROM quay.io/keycloak/keycloak:26.6.0
COPY --from=theme /theme/dist_keycloak/orderflow-keycloak-theme.jar /opt/keycloak/providers/
