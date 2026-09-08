# Base stage for runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy all csproj files and restore as distinct layers (Layer caching)
COPY ["src/ApiGateways/Gateway.API/Gateway.API.csproj", "src/ApiGateways/Gateway.API/"]
COPY ["src/BuildingBlocks/Core/BuildingBlocks.Core/BuildingBlocks.Core.csproj", "src/BuildingBlocks/Core/BuildingBlocks.Core/"]
RUN dotnet restore "src/ApiGateways/Gateway.API/Gateway.API.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/src/ApiGateways/Gateway.API"
RUN dotnet build "Gateway.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Gateway.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gateway.API.dll"]
