FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Vicinia.HistoryService.csproj", "./"]
RUN dotnet restore "Vicinia.HistoryService.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Vicinia.HistoryService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Vicinia.HistoryService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Vicinia.HistoryService.dll"]
