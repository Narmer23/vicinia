using Microsoft.AspNetCore.Mvc;
using Serilog;
using Vicinia.PoiService.Models;
using Newtonsoft.Json;

namespace Vicinia.PoiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PoiController : ControllerBase
{
    private readonly ILogger<PoiController> _logger;
    private readonly HttpClient _httpClient;

    public PoiController(ILogger<PoiController> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchPois([FromBody] PoiSearchRequest request)
    {
        try
        {
            _logger.LogInformation("Searching POIs at Lat: {Lat}, Lon: {Lon}, Radius: {Radius}km", 
                request.Latitude, request.Longitude, request.RadiusKm);

            // This is a placeholder implementation
            // In production, you would integrate with Regione Lombardia's OpenData APIs
            var pois = await GetPoisFromLombardiaApi(request);

            var response = new PoiSearchResponse
            {
                Pois = pois,
                TotalCount = pois.Count,
                SearchRadiusKm = request.RadiusKm,
                SearchLocation = $"Lat: {request.Latitude}, Lon: {request.Longitude}"
            };

            _logger.LogInformation("Found {Count} POIs", pois.Count);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching POIs");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    [HttpGet("categories")]
    public IActionResult GetCategories()
    {
        var categories = new List<string>
        {
            "schools",
            "hospitals", 
            "supermarkets",
            "pharmacies",
            "banks",
            "post_offices",
            "police_stations",
            "fire_stations",
            "libraries",
            "museums",
            "parks",
            "restaurants",
            "shopping_centers"
        };

        return Ok(categories);
    }

    private async Task<List<Poi>> GetPoisFromLombardiaApi(PoiSearchRequest request)
    {
        // This is a mock implementation
        // In production, you would call the actual Regione Lombardia OpenData APIs
        
        var mockPois = new List<Poi>();
        var random = new Random();

        // Generate mock POIs around the requested location
        for (int i = 0; i < Math.Min(request.MaxResults, 20); i++)
        {
            var categories = new[] { "schools", "hospitals", "supermarkets", "pharmacies" };
            var category = categories[random.Next(categories.Length)];

            // Generate random coordinates within the radius
            var angle = random.NextDouble() * 2 * Math.PI;
            var distance = random.NextDouble() * request.RadiusKm;
            
            var latOffset = distance * Math.Cos(angle) / 111.0; // Approximate km to degrees
            var lonOffset = distance * Math.Sin(angle) / (111.0 * Math.Cos(request.Latitude * Math.PI / 180));

            var poi = new Poi
            {
                Id = $"poi_{i}",
                Name = $"{category.Substring(0, 1).ToUpper() + category.Substring(1)} {i + 1}",
                Category = category,
                Latitude = request.Latitude + latOffset,
                Longitude = request.Longitude + lonOffset,
                Address = $"Via Example {i + 1}, Lombardy, Italy",
                Description = $"A {category} in the Lombardy region"
            };

            mockPois.Add(poi);
        }

        // Filter by categories if specified
        if (request.Categories?.Any() == true)
        {
            mockPois = mockPois.Where(p => request.Categories.Contains(p.Category)).ToList();
        }

        return mockPois;
    }
} 