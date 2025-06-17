using Microsoft.AspNetCore.Mvc;
using Serilog;
using Vicinia.ScoringService.Models;
using Vicinia.ScoringService.Services;

namespace Vicinia.ScoringService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoringController : ControllerBase
{
    private readonly ILogger<ScoringController> _logger;
    private readonly IGeocodingServiceClient _geocodingServiceClient;
    private readonly IPoiServiceClient _poiServiceClient;
    private readonly IHistoryServiceClient _historyServiceClient;
    private readonly ScoringService _scoringService;

    public ScoringController(
        ILogger<ScoringController> logger,
        IGeocodingServiceClient geocodingServiceClient,
        IPoiServiceClient poiServiceClient,
        IHistoryServiceClient historyServiceClient)
    {
        _logger = logger;
        _geocodingServiceClient = geocodingServiceClient;
        _poiServiceClient = poiServiceClient;
        _historyServiceClient = historyServiceClient;
        _scoringService = new ScoringService();
    }

    [HttpPost("calculate")]
    public async Task<IActionResult> CalculateScore([FromBody] ScoringRequest request)
    {
        try
        {
            _logger.LogInformation("Calculating score for address: {Address}, Mode: {Mode}", 
                request.Address, request.TransportationMode);

            // Step 1: Geocode the address
            var geocodingResponse = await _geocodingServiceClient.GeocodeAddressAsync(request.Address);
            if (geocodingResponse == null)
            {
                return BadRequest(new { Message = "Could not geocode the provided address" });
            }

            // Step 2: Get nearby POIs
            var radiusKm = GetRadiusForTransportationMode(request.TransportationMode);
            var pois = await _poiServiceClient.GetNearbyPoisAsync(
                geocodingResponse.Latitude, 
                geocodingResponse.Longitude, 
                radiusKm);

            if (pois == null || !pois.Any())
            {
                return Ok(new ScoringResponse
                {
                    OverallScore = 0,
                    CategoryScores = new Dictionary<string, double>(),
                    PoiScores = new List<PoiScore>(),
                    TransportationMode = request.TransportationMode,
                    Location = geocodingResponse.FormattedAddress ?? request.Address,
                    Message = "No POIs found in the specified radius"
                });
            }

            // Step 3: Convert POIs to PoiDistance format for scoring
            var poiDistances = pois.Select(p => new PoiDistance
            {
                PoiId = p.Id.ToString(),
                Category = p.Type,
                Name = p.Name,
                DistanceKm = p.DistanceKm
            }).ToList();

            // Step 4: Calculate scores
            var scoringRequest = new ScoringRequest
            {
                Address = request.Address,
                Latitude = geocodingResponse.Latitude,
                Longitude = geocodingResponse.Longitude,
                TransportationMode = request.TransportationMode,
                PoiDistances = poiDistances
            };

            var response = _scoringService.CalculateScore(scoringRequest);
            response.Location = geocodingResponse.FormattedAddress ?? request.Address;

            // Step 5: Save to history (if user is authenticated)
            if (request.UserId.HasValue)
            {
                var historyRequest = new SearchHistoryRequest
                {
                    UserId = request.UserId,
                    Address = request.Address,
                    Latitude = geocodingResponse.Latitude,
                    Longitude = geocodingResponse.Longitude,
                    TransportationMode = request.TransportationMode,
                    OverallScore = response.OverallScore,
                    PoiCount = pois.Count
                };

                await _historyServiceClient.SaveSearchHistoryAsync(historyRequest);
            }

            _logger.LogInformation("Score calculated: {Score} for {PoiCount} POIs", 
                response.OverallScore, pois.Count);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating score");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    [HttpPost("calculate-with-coordinates")]
    public async Task<IActionResult> CalculateScoreWithCoordinates([FromBody] ScoringRequestWithCoordinates request)
    {
        try
        {
            _logger.LogInformation("Calculating score for coordinates: Lat: {Lat}, Lon: {Lon}, Mode: {Mode}", 
                request.Latitude, request.Longitude, request.TransportationMode);

            // Step 1: Get nearby POIs
            var radiusKm = GetRadiusForTransportationMode(request.TransportationMode);
            var pois = await _poiServiceClient.GetNearbyPoisAsync(
                request.Latitude, 
                request.Longitude, 
                radiusKm);

            if (pois == null || !pois.Any())
            {
                return Ok(new ScoringResponse
                {
                    OverallScore = 0,
                    CategoryScores = new Dictionary<string, double>(),
                    PoiScores = new List<PoiScore>(),
                    TransportationMode = request.TransportationMode,
                    Location = $"Lat: {request.Latitude}, Lon: {request.Longitude}",
                    Message = "No POIs found in the specified radius"
                });
            }

            // Step 2: Convert POIs to PoiDistance format for scoring
            var poiDistances = pois.Select(p => new PoiDistance
            {
                PoiId = p.Id.ToString(),
                Category = p.Type,
                Name = p.Name,
                DistanceKm = p.DistanceKm
            }).ToList();

            // Step 3: Calculate scores
            var scoringRequest = new ScoringRequest
            {
                Address = request.Address ?? $"Lat: {request.Latitude}, Lon: {request.Longitude}",
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                TransportationMode = request.TransportationMode,
                PoiDistances = poiDistances
            };

            var response = _scoringService.CalculateScore(scoringRequest);

            // Step 4: Save to history (if user is authenticated)
            if (request.UserId.HasValue)
            {
                var historyRequest = new SearchHistoryRequest
                {
                    UserId = request.UserId,
                    Address = request.Address ?? $"Lat: {request.Latitude}, Lon: {request.Longitude}",
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    TransportationMode = request.TransportationMode,
                    OverallScore = response.OverallScore,
                    PoiCount = pois.Count
                };

                await _historyServiceClient.SaveSearchHistoryAsync(historyRequest);
            }

            _logger.LogInformation("Score calculated: {Score} for {PoiCount} POIs", 
                response.OverallScore, pois.Count);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating score");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    [HttpGet("formulas")]
    public IActionResult GetScoringFormulas()
    {
        var formulas = new List<ScoringFormula>
        {
            new() { Category = "schools", BaseScore = 10.0, MaxDistanceKm = 3.0, Weight = 1.2, Formula = "linear" },
            new() { Category = "hospitals", BaseScore = 10.0, MaxDistanceKm = 5.0, Weight = 1.5, Formula = "linear" },
            new() { Category = "supermarkets", BaseScore = 10.0, MaxDistanceKm = 2.0, Weight = 1.0, Formula = "linear" },
            new() { Category = "pharmacies", BaseScore = 10.0, MaxDistanceKm = 1.5, Weight = 0.8, Formula = "linear" },
            new() { Category = "banks", BaseScore = 10.0, MaxDistanceKm = 2.0, Weight = 0.6, Formula = "linear" },
            new() { Category = "post_offices", BaseScore = 10.0, MaxDistanceKm = 3.0, Weight = 0.5, Formula = "linear" }
        };

        return Ok(formulas);
    }

    private double GetRadiusForTransportationMode(string transportationMode)
    {
        return transportationMode.ToLower() switch
        {
            "walking" => 2.0,
            "cycling" => 5.0,
            "driving" => 10.0,
            "public_transport" => 8.0,
            _ => 5.0
        };
    }
}

public class ScoringRequestWithCoordinates
{
    public string? Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string TransportationMode { get; set; } = "walking";
    public Guid? UserId { get; set; }
}

public class ScoringService
{
    private readonly Dictionary<string, ScoringFormula> _formulas;

    public ScoringService()
    {
        _formulas = new Dictionary<string, ScoringFormula>
        {
            ["schools"] = new() { Category = "schools", BaseScore = 10.0, MaxDistanceKm = 3.0, Weight = 1.2, Formula = "linear" },
            ["hospitals"] = new() { Category = "hospitals", BaseScore = 10.0, MaxDistanceKm = 5.0, Weight = 1.5, Formula = "linear" },
            ["supermarkets"] = new() { Category = "supermarkets", BaseScore = 10.0, MaxDistanceKm = 2.0, Weight = 1.0, Formula = "linear" },
            ["pharmacies"] = new() { Category = "pharmacies", BaseScore = 10.0, MaxDistanceKm = 1.5, Weight = 0.8, Formula = "linear" },
            ["banks"] = new() { Category = "banks", BaseScore = 10.0, MaxDistanceKm = 2.0, Weight = 0.6, Formula = "linear" },
            ["post_offices"] = new() { Category = "post_offices", BaseScore = 10.0, MaxDistanceKm = 3.0, Weight = 0.5, Formula = "linear" }
        };
    }

    public ScoringResponse CalculateScore(ScoringRequest request)
    {
        var poiScores = new List<PoiScore>();
        var categoryScores = new Dictionary<string, List<double>>();

        foreach (var poiDistance in request.PoiDistances)
        {
            var formula = _formulas.GetValueOrDefault(poiDistance.Category, 
                new() { BaseScore = 10.0, MaxDistanceKm = 5.0, Weight = 1.0, Formula = "linear" });

            var score = CalculatePoiScore(poiDistance.DistanceKm, formula);
            var poiScore = new PoiScore
            {
                PoiId = poiDistance.PoiId,
                Category = poiDistance.Category,
                Name = poiDistance.Name,
                DistanceKm = poiDistance.DistanceKm,
                Score = score,
                Weight = formula.Weight
            };

            poiScores.Add(poiScore);

            if (!categoryScores.ContainsKey(poiDistance.Category))
            {
                categoryScores[poiDistance.Category] = new List<double>();
            }
            categoryScores[poiDistance.Category].Add(score);
        }

        // Calculate category scores (average of all POIs in that category)
        var categoryAverages = new Dictionary<string, double>();
        foreach (var category in categoryScores)
        {
            categoryAverages[category.Key] = category.Value.Average();
        }

        // Calculate overall score (weighted average of category scores)
        var overallScore = CalculateOverallScore(categoryAverages);

        return new ScoringResponse
        {
            OverallScore = Math.Round(overallScore, 2),
            CategoryScores = categoryAverages.ToDictionary(kvp => kvp.Key, kvp => Math.Round(kvp.Value, 2)),
            PoiScores = poiScores,
            TransportationMode = request.TransportationMode,
            Location = $"Lat: {request.Latitude}, Lon: {request.Longitude}"
        };
    }

    private double CalculatePoiScore(double distanceKm, ScoringFormula formula)
    {
        if (distanceKm <= 0) return formula.BaseScore;
        if (distanceKm >= formula.MaxDistanceKm) return 0;

        // Linear scoring formula
        var score = formula.BaseScore * (1 - (distanceKm / formula.MaxDistanceKm));
        return Math.Max(0, Math.Min(10, score));
    }

    private double CalculateOverallScore(Dictionary<string, double> categoryScores)
    {
        if (!categoryScores.Any()) return 0;

        var totalWeight = 0.0;
        var weightedSum = 0.0;

        foreach (var category in categoryScores)
        {
            var formula = _formulas.GetValueOrDefault(category.Key, 
                new() { Weight = 1.0 });
            
            totalWeight += formula.Weight;
            weightedSum += category.Value * formula.Weight;
        }

        return totalWeight > 0 ? weightedSum / totalWeight : 0;
    }
} 