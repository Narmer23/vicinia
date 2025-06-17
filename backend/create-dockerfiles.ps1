# Create Dockerfiles for all Vicinia microservices
$services = @(
    "Vicinia.UserService",
    "Vicinia.ScoringService",
    "Vicinia.GeocodingService",
    "Vicinia.PoiService",
    "Vicinia.HistoryService",
    "Vicinia.LoggingService"
)

$dockerfileContent = @"
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["{0}.csproj", "./"]
RUN dotnet restore "{0}.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "{0}.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "{0}.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "{0}.dll"]
"@

foreach ($service in $services) {
    Write-Host "Creating Dockerfile for $service..."
    $dockerfilePath = "src\$service\Dockerfile"
    $content = $dockerfileContent -f $service
    Set-Content -Path $dockerfilePath -Value $content
    Write-Host "Dockerfile created for $service"
}

Write-Host "All Dockerfiles created!" 