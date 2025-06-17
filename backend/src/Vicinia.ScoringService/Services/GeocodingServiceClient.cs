using System.Text.Json;
using Vicinia.ScoringService.Models;

namespace Vicinia.ScoringService.Services;

public interface IGeocodingServiceClient
{
    Task<GeocodingResponse?> GeocodeAddressAsync(string address);
}

public class GeocodingServiceClient : IGeocodingServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeocodingServiceClient> _logger;
    private readonly IServiceUrlResolver _serviceUrlResolver;

    public GeocodingServiceClient(HttpClient httpClient, ILogger<GeocodingServiceClient> logger, IServiceUrlResolver serviceUrlResolver)
    {
        _httpClient = httpClient;
        _logger = logger;
        _serviceUrlResolver = serviceUrlResolver;
    }

    public async Task<GeocodingResponse?> GeocodeAddressAsync(string address)
    {
        try
        {
            _logger.LogInformation("Geocoding address: {Address}", address);

            var geocodingServiceUrl = _serviceUrlResolver.GetServiceUrl("GeocodingService");
            var request = new { Address = address };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{geocodingServiceUrl}/api/geocoding/geocode", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var geocodingResponse = JsonSerializer.Deserialize<GeocodingResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Successfully geocoded address: {Address} -> Lat: {Lat}, Lng: {Lng}", 
                    address, geocodingResponse?.Latitude, geocodingResponse?.Longitude);

                return geocodingResponse;
            }
            else
            {
                _logger.LogWarning("Failed to geocode address: {Address}. Status: {StatusCode}", 
                    address, response.StatusCode);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error geocoding address: {Address}", address);
            return null;
        }
    }
}

public class GeocodingResponse
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? FormattedAddress { get; set; }
} 