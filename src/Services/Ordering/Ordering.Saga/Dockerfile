FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/Ordering/Ordering.Saga/Ordering.Saga.csproj", "src/Services/Ordering/Ordering.Saga/"]
COPY ["src/BuildingBlocks/Core/BuildingBlocks.Core/BuildingBlocks.Core.csproj", "src/BuildingBlocks/Core/BuildingBlocks.Core/"]
COPY ["src/BuildingBlocks/Messaging/BuildingBlocks.Messaging.MassTransit/BuildingBlocks.Messaging.MassTransit.csproj", "src/BuildingBlocks/Messaging/BuildingBlocks.Messaging.MassTransit/"]
COPY ["src/BuildingBlocks/Observability/BuildingBlocks.Observability/BuildingBlocks.Observability.csproj", "src/BuildingBlocks/Observability/BuildingBlocks.Observability/"]
COPY ["src/BuildingBlocks/Persistence/BuildingBlocks.Persistence.EntityFrameworkCore/BuildingBlocks.Persistence.EntityFrameworkCore.csproj", "src/BuildingBlocks/Persistence/BuildingBlocks.Persistence.EntityFrameworkCore/"]
COPY ["src/BuildingBlocks/Persistence/BuildingBlocks.Persistence.PostgreSql/BuildingBlocks.Persistence.PostgreSql.csproj", "src/BuildingBlocks/Persistence/BuildingBlocks.Persistence.PostgreSql/"]
COPY ["src/Services/Ordering/Ordering.Contracts/Ordering.Contracts.csproj", "src/Services/Ordering/Ordering.Contracts/"]
COPY ["src/Services/Inventory/Inventory.Contracts/Inventory.Contracts.csproj", "src/Services/Inventory/Inventory.Contracts/"]
COPY ["src/Services/Payment/Payment.Contracts/Payment.Contracts.csproj", "src/Services/Payment/Payment.Contracts/"]
RUN dotnet restore "src/Services/Ordering/Ordering.Saga/Ordering.Saga.csproj"
COPY . .
WORKDIR "/src/src/Services/Ordering/Ordering.Saga"
RUN dotnet build "Ordering.Saga.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ordering.Saga.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ordering.Saga.dll"]
