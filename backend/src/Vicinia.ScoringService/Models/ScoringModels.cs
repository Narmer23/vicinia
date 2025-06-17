namespace Vicinia.ScoringService.Models;

public class ScoringRequest
{
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string TransportationMode { get; set; } = "car"; // "car" or "walking"
    public List<PoiDistance> PoiDistances { get; set; } = new();
    public Guid? UserId { get; set; }
}

public class PoiDistance
{
    public string PoiId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ScoringResponse
{
    public double OverallScore { get; set; }
    public Dictionary<string, double> CategoryScores { get; set; } = new();
    public List<PoiScore> PoiScores { get; set; } = new();
    public string TransportationMode { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Message { get; set; }
}

public class PoiScore
{
    public string PoiId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public double Score { get; set; }
    public double Weight { get; set; }
}

public class ScoringFormula
{
    public string Category { get; set; } = string.Empty;
    public double BaseScore { get; set; } = 10.0;
    public double MaxDistanceKm { get; set; } = 5.0;
    public double Weight { get; set; } = 1.0;
    public string Formula { get; set; } = "linear"; // "linear", "exponential", "custom"
} 