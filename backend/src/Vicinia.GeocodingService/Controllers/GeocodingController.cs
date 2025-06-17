using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Vicinia.GeocodingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeocodingController : ControllerBase
{
    private readonly ILogger<GeocodingController> _logger;
    private readonly HttpClient _httpClient;

    public GeocodingController(ILogger<GeocodingController> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    [HttpGet("geocode")]
    public async Task<IActionResult> GeocodeAddress([FromQuery] string address)
    {
        try
        {
            _logger.LogInformation("Geocoding address: {Address}", address);

            // Use OpenStreetMap Nominatim for geocoding
            var encodedAddress = Uri.EscapeDataString(address + ", Lombardy, Italy");
            var url = $"https://nominatim.openstreetmap.org/search?q={encodedAddress}&format=json&limit=1";

            var response = await _httpClient.GetStringAsync(url);
            
            // Parse the response (simplified - in production you'd use proper JSON parsing)
            if (response.Contains("lat") && response.Contains("lon"))
            {
                // Extract coordinates from the response
                var latMatch = System.Text.RegularExpressions.Regex.Match(response, @"""lat"":""([^""]+)""");
                var lonMatch = System.Text.RegularExpressions.Regex.Match(response, @"""lon"":""([^""]+)""");

                if (latMatch.Success && lonMatch.Success)
                {
                    var latitude = double.Parse(latMatch.Groups[1].Value);
                    var longitude = double.Parse(lonMatch.Groups[1].Value);

                    var result = new
                    {
                        Address = address,
                        Latitude = latitude,
                        Longitude = longitude,
                        Success = true
                    };

                    _logger.LogInformation("Successfully geocoded address: {Address} -> Lat: {Lat}, Lon: {Lon}", 
                        address, latitude, longitude);

                    return Ok(result);
                }
            }

            _logger.LogWarning("Failed to geocode address: {Address}", address);
            return NotFound(new { Message = "Address not found", Success = false });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error geocoding address: {Address}", address);
            return StatusCode(500, new { Message = "Internal server error", Success = false });
        }
    }

    [HttpGet("reverse-geocode")]
    public async Task<IActionResult> ReverseGeocode([FromQuery] double lat, [FromQuery] double lon)
    {
        try
        {
            _logger.LogInformation("Reverse geocoding coordinates: Lat: {Lat}, Lon: {Lon}", lat, lon);

            var url = $"https://nominatim.openstreetmap.org/reverse?lat={lat}&lon={lon}&format=json";

            var response = await _httpClient.GetStringAsync(url);
            
            // Parse the response (simplified)
            var displayNameMatch = System.Text.RegularExpressions.Regex.Match(response, @"""display_name"":""([^""]+)""");

            if (displayNameMatch.Success)
            {
                var address = displayNameMatch.Groups[1].Value;

                var result = new
                {
                    Latitude = lat,
                    Longitude = lon,
                    Address = address,
                    Success = true
                };

                _logger.LogInformation("Successfully reverse geocoded: Lat: {Lat}, Lon: {Lon} -> {Address}", 
                    lat, lon, address);

                return Ok(result);
            }

            _logger.LogWarning("Failed to reverse geocode coordinates: Lat: {Lat}, Lon: {Lon}", lat, lon);
            return NotFound(new { Message = "Location not found", Success = false });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reverse geocoding coordinates: Lat: {Lat}, Lon: {Lon}", lat, lon);
            return StatusCode(500, new { Message = "Internal server error", Success = false });
        }
    }
} 