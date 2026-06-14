# ---- Stage 1: Build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies (layer cache optimization)
COPY src/WastePlatform.Domain/WastePlatform.Domain.csproj src/WastePlatform.Domain/
COPY src/WastePlatform.Application/WastePlatform.Application.csproj src/WastePlatform.Application/
COPY src/WastePlatform.Infrastructure/WastePlatform.Infrastructure.csproj src/WastePlatform.Infrastructure/
COPY src/WastePlatform.API/WastePlatform.API.csproj src/WastePlatform.API/

RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore src/WastePlatform.API/WastePlatform.API.csproj

# Copy all source and build
COPY . .
WORKDIR /src/src/WastePlatform.API
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish -c Release -o /app/publish /p:PublishTrimmed=false

# ---- Stage 2: Runtime (Debian slim for BCrypt compatibility) ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# ── Memory optimization for Render Free Tier (512MB RAM) ──
# Use Workstation GC instead of Server GC (saves ~200MB RAM)
ENV DOTNET_gcServer=0
# Limit GC heap to 200MB to prevent OOM kills
ENV DOTNET_GCHeapHardLimit=0xC800000
# Maximum memory conservation
ENV DOTNET_GCConserveMemory=9
# Reduce thread pool overhead
ENV DOTNET_ThreadPool_UnfairSemaphoreSpinLimit=0
# Disable tiered compilation to reduce JIT memory
ENV DOTNET_TieredCompilation=0

ENTRYPOINT ["dotnet", "WastePlatform.API.dll"]
