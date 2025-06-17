# Create all Vicinia microservices
$services = @(
    "Vicinia.GeocodingService",
    "Vicinia.PoiService", 
    "Vicinia.HistoryService",
    "Vicinia.LoggingService"
)

foreach ($service in $services) {
    Write-Host "Creating $service..."
    cd "src\$service"
    dotnet new webapi -n $service
    Move-Item -Path "$service\*" -Destination "." -Force
    Remove-Item -Path $service -Recurse -Force
    cd ..\..
    dotnet sln add "src\$service\$service.csproj"
    Write-Host "$service created successfully!"
}

Write-Host "All services created!" 