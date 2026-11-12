# CashStack API as a container (Render has no native .NET runtime).
# Build:  docker build -t cashstack-api .
# Run:    docker run --rm -p 8080:8080 cashstack-api   →  http://localhost:8080/health

# ---------- Build stage: full SDK, restore + publish ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Project files first: the restore layer stays cached until a package reference changes
COPY src/CashStack.Core/CashStack.Core.csproj src/CashStack.Core/
COPY src/CashStack.Api/CashStack.Api.csproj src/CashStack.Api/
RUN dotnet restore src/CashStack.Api/CashStack.Api.csproj

COPY src/CashStack.Core/ src/CashStack.Core/
COPY src/CashStack.Api/ src/CashStack.Api/
RUN dotnet publish src/CashStack.Api/CashStack.Api.csproj --configuration Release --no-restore --output /app

# ---------- Runtime stage: only the ASP.NET Core runtime and the published app ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# The app writes the SQLite file and the audit log relative to /app.
# Create the folders as root, then hand them to the image's non-root user.
RUN mkdir -p data logs && chown "$APP_UID" data logs
USER $APP_UID

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Render passes the port in $PORT; locally the default 8080 is used.
# exec: dotnet becomes PID 1 and receives SIGTERM for a graceful shutdown.
ENTRYPOINT ["/bin/sh", "-c", "ASPNETCORE_HTTP_PORTS=${PORT:-8080} exec dotnet CashStack.Api.dll"]
