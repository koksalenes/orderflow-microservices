# syntax=docker/dockerfile:1

# One Dockerfile for every Api and Worker host:
#   docker build -f deploy/docker/Dockerfile --build-arg PROJECT=src/Services/Orders/OrderFlow.Orders.Api .
# Multi-arch: the SDK stage runs on the build machine and cross-compiles for TARGETARCH (no emulation).

ARG SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:10.0.401
ARG RUNTIME_IMAGE=mcr.microsoft.com/dotnet/aspnet:10.0.13-noble-chiseled

FROM --platform=$BUILDPLATFORM ${SDK_IMAGE} AS build
ARG PROJECT
ARG TARGETARCH
ENV HUSKY=0 \
    DOTNET_NOLOGO=1 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1
WORKDIR /source

COPY global.json Directory.Build.props Directory.Packages.props BannedSymbols.txt .editorconfig ./
COPY src/ src/

RUN --mount=type=cache,id=orderflow-nuget,target=/root/.nuget/packages,sharing=locked \
    test -n "${PROJECT}" || (echo "Build argument PROJECT is required." && exit 1); \
    case "${TARGETARCH:-amd64}" in amd64) arch=x64 ;; *) arch="${TARGETARCH}" ;; esac; \
    dotnet publish "${PROJECT}" --configuration Release --arch "${arch}" --self-contained false --output /app && \
    cp "/app/$(basename "${PROJECT}")" /app/host

FROM ${RUNTIME_IMAGE} AS runtime
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080
HEALTHCHECK --interval=10s --timeout=5s --start-period=20s --retries=5 \
    CMD ["/app/host", "health-probe", "/health/ready"]
ENTRYPOINT ["/app/host"]
