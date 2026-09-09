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

# Copy csproj files for layer caching
COPY ["src/Services/Basket/Basket.API/Basket.API.csproj", "src/Services/Basket/Basket.API/"]
COPY ["src/BuildingBlocks/Core/BuildingBlocks.Core/BuildingBlocks.Core.csproj", "src/BuildingBlocks/Core/BuildingBlocks.Core/"]
COPY ["src/BuildingBlocks/Core/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/Core/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/WebApi/BuildingBlocks.Web/BuildingBlocks.Web.csproj", "src/BuildingBlocks/WebApi/BuildingBlocks.Web/"]
COPY ["src/BuildingBlocks/Persistence/BuildingBlocks.Persistence.Marten/BuildingBlocks.Persistence.Marten.csproj", "src/BuildingBlocks/Persistence/BuildingBlocks.Persistence.Marten/"]

RUN dotnet restore "src/Services/Basket/Basket.API/Basket.API.csproj"

# Copy all source code
COPY . .
WORKDIR "/src/src/Services/Basket/Basket.API"
RUN dotnet build "Basket.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Basket.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Basket.API.dll"]
