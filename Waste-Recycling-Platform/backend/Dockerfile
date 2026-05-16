# ---- Stage 1: Build ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies (layer cache optimization)
COPY src/WastePlatform.Domain/WastePlatform.Domain.csproj src/WastePlatform.Domain/
COPY src/WastePlatform.Application/WastePlatform.Application.csproj src/WastePlatform.Application/
COPY src/WastePlatform.Infrastructure/WastePlatform.Infrastructure.csproj src/WastePlatform.Infrastructure/
COPY src/WastePlatform.API/WastePlatform.API.csproj src/WastePlatform.API/

RUN dotnet restore src/WastePlatform.API/WastePlatform.API.csproj

# Copy all source and build
COPY . .
WORKDIR /src/src/WastePlatform.API
RUN dotnet publish -c Release -o /app/publish

# ---- Stage 2: Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "WastePlatform.API.dll"]
