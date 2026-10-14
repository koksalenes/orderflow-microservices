# syntax=docker/dockerfile:1

# BFF image: the React app (web/app) is built and served as static files by the BFF (same origin).
# Multi-arch: node and SDK stages run on the build machine; .NET cross-compiles for TARGETARCH.

ARG SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:10.0.401
ARG RUNTIME_IMAGE=mcr.microsoft.com/dotnet/aspnet:10.0.13-noble-chiseled

FROM --platform=$BUILDPLATFORM node:24.15.0-bookworm-slim AS web
WORKDIR /web
COPY web/app/package.json web/app/package-lock.json ./
RUN --mount=type=cache,id=orderflow-npm,target=/root/.npm npm ci
COPY web/app/ ./
RUN npm run build

FROM --platform=$BUILDPLATFORM ${SDK_IMAGE} AS build
ARG TARGETARCH
ENV HUSKY=0 \
    DOTNET_NOLOGO=1 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1
WORKDIR /source
COPY global.json Directory.Build.props Directory.Packages.props BannedSymbols.txt .editorconfig ./
COPY src/ src/
RUN --mount=type=cache,id=orderflow-nuget,target=/root/.nuget/packages,sharing=locked \
    case "${TARGETARCH:-amd64}" in amd64) arch=x64 ;; *) arch="${TARGETARCH}" ;; esac; \
    dotnet publish src/Bff/OrderFlow.Bff --configuration Release --arch "${arch}" --self-contained false --output /app && \
    cp /app/OrderFlow.Bff /app/host

FROM ${RUNTIME_IMAGE} AS runtime
WORKDIR /app
COPY --from=build /app .
COPY --from=web /web/dist ./wwwroot

EXPOSE 8080
HEALTHCHECK --interval=10s --timeout=5s --start-period=20s --retries=5 \
    CMD ["/app/host", "health-probe", "/health/ready"]
ENTRYPOINT ["/app/host"]
