# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

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
COPY ["src/Services/Catalog/Catalog.API/Catalog.API.csproj", "src/Services/Catalog/Catalog.API/"]
COPY ["src/BuildingBlocks/Core/BuildingBlocks.Core/BuildingBlocks.Core.csproj", "src/BuildingBlocks/Core/BuildingBlocks.Core/"]
COPY ["src/BuildingBlocks/Core/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/Core/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/WebApi/BuildingBlocks.Web/BuildingBlocks.Web.csproj", "src/BuildingBlocks/WebApi/BuildingBlocks.Web/"]
COPY ["src/BuildingBlocks/Persistence/BuildingBlocks.Persistence.EntityFrameworkCore/BuildingBlocks.Persistence.EntityFrameworkCore.csproj", "src/BuildingBlocks/Persistence/BuildingBlocks.Persistence.EntityFrameworkCore/"]
COPY ["src/BuildingBlocks/Persistence/BuildingBlocks.Persistence.PostgreSql/BuildingBlocks.Persistence.PostgreSql.csproj", "src/BuildingBlocks/Persistence/BuildingBlocks.Persistence.PostgreSql/"]
RUN dotnet restore "src/Services/Catalog/Catalog.API/Catalog.API.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/src/Services/Catalog/Catalog.API"
RUN dotnet build "Catalog.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Catalog.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalog.API.dll"]