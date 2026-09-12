FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/Notification/Notification.API/Notification.API.csproj", "src/Services/Notification/Notification.API/"]
COPY ["src/Services/Identity/Identity.Contracts/Identity.Contracts.csproj", "src/Services/Identity/Identity.Contracts/"]
COPY ["src/BuildingBlocks/", "src/BuildingBlocks/"]
RUN dotnet restore "src/Services/Notification/Notification.API/Notification.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Notification/Notification.API"
RUN dotnet build "Notification.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notification.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.API.dll"]
