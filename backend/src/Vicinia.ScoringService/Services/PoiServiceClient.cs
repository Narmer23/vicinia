using System.Text.Json;
using Vicinia.ScoringService.Models;

namespace Vicinia.ScoringService.Services;

public interface IPoiServiceClient
{
    Task<List<PoiResponse>?> GetNearbyPoisAsync(double latitude, double longitude, double radiusKm);
}

public class PoiServiceClient : IPoiServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PoiServiceClient> _logger;
    private readonly string _poiServiceUrl;

    public PoiServiceClient(HttpClient httpClient, ILogger<PoiServiceClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _poiServiceUrl = configuration["Services:PoiService"] ?? "http://poi-service:5003";
    }

    public async Task<List<PoiResponse>?> GetNearbyPoisAsync(double latitude, double longitude, double radiusKm)
    {
        try
        {
            _logger.LogInformation("Getting nearby POIs for location: Lat: {Lat}, Lng: {Lng}, Radius: {Radius}km", 
                latitude, longitude, radiusKm);

            var url = $"{_poiServiceUrl}/api/poi/nearby?latitude={latitude}&longitude={longitude}&radiusKm={radiusKm}";
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var pois = JsonSerializer.Deserialize<List<PoiResponse>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Successfully retrieved {Count} nearby POIs", pois?.Count ?? 0);
                return pois;
            }
            else
            {
                _logger.LogWarning("Failed to get nearby POIs. Status: {StatusCode}", response.StatusCode);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting nearby POIs for location: Lat: {Lat}, Lng: {Lng}", latitude, longitude);
            return null;
        }
    }
}

public class PoiResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public int Score { get; set; }
} 