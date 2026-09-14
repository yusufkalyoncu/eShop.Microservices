FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Services/Notification/Notification.API/Notification.API.csproj", "src/Services/Notification/Notification.API/"]
COPY ["src/Services/Identity/Identity.Contracts/Identity.Contracts.csproj", "src/Services/Identity/Identity.Contracts/"]
COPY ["src/BuildingBlocks/Core/BuildingBlocks.Core/BuildingBlocks.Core.csproj", "src/BuildingBlocks/Core/BuildingBlocks.Core/"]
COPY ["src/BuildingBlocks/Core/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/Core/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/WebApi/BuildingBlocks.Web/BuildingBlocks.Web.csproj", "src/BuildingBlocks/WebApi/BuildingBlocks.Web/"]
COPY ["src/BuildingBlocks/Messaging/BuildingBlocks.Messaging.Abstractions/BuildingBlocks.Messaging.Abstractions.csproj", "src/BuildingBlocks/Messaging/BuildingBlocks.Messaging.Abstractions/"]
COPY ["src/BuildingBlocks/Messaging/BuildingBlocks.Messaging.MassTransit/BuildingBlocks.Messaging.MassTransit.csproj", "src/BuildingBlocks/Messaging/BuildingBlocks.Messaging.MassTransit/"]
COPY ["src/BuildingBlocks/Inbox/BuildingBlocks.Inbox.Abstractions/BuildingBlocks.Inbox.Abstractions.csproj", "src/BuildingBlocks/Inbox/BuildingBlocks.Inbox.Abstractions/"]
COPY ["src/BuildingBlocks/Inbox/BuildingBlocks.Inbox.EntityFrameworkCore/BuildingBlocks.Inbox.EntityFrameworkCore.csproj", "src/BuildingBlocks/Inbox/BuildingBlocks.Inbox.EntityFrameworkCore/"]
COPY ["src/BuildingBlocks/Inbox/BuildingBlocks.Inbox.PostgreSql/BuildingBlocks.Inbox.PostgreSql.csproj", "src/BuildingBlocks/Inbox/BuildingBlocks.Inbox.PostgreSql/"]
COPY ["src/BuildingBlocks/Persistence/BuildingBlocks.Persistence.EntityFrameworkCore/BuildingBlocks.Persistence.EntityFrameworkCore.csproj", "src/BuildingBlocks/Persistence/BuildingBlocks.Persistence.EntityFrameworkCore/"]
COPY ["src/BuildingBlocks/Persistence/BuildingBlocks.Persistence.PostgreSql/BuildingBlocks.Persistence.PostgreSql.csproj", "src/BuildingBlocks/Persistence/BuildingBlocks.Persistence.PostgreSql/"]
COPY ["src/BuildingBlocks/Observability/BuildingBlocks.Observability/BuildingBlocks.Observability.csproj", "src/BuildingBlocks/Observability/BuildingBlocks.Observability/"]

RUN dotnet restore "src/Services/Notification/Notification.API/Notification.API.csproj"

COPY . .
WORKDIR "/src/src/Services/Notification/Notification.API"
RUN dotnet build "Notification.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Notification.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.API.dll"]
