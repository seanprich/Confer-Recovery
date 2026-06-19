FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Server/ConferRecovery.Server.csproj", "src/Server/"]
RUN dotnet restore "src/Server/ConferRecovery.Server.csproj"

COPY . .
WORKDIR "/src/src/Server"
RUN dotnet publish "ConferRecovery.Server.csproj" \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Non-root user for production hardening
RUN groupadd --gid 1001 confer && useradd --uid 1001 --gid confer --shell /bin/bash confer
RUN mkdir -p /app/logs && chown -R confer:confer /app

COPY --from=build --chown=confer:confer /app/publish .

USER confer
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:8080/healthz || exit 1

ENTRYPOINT ["dotnet", "ConferRecovery.Server.dll"]
