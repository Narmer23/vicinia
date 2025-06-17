namespace Vicinia.PoiService.Models;

public class Poi
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object>? AdditionalProperties { get; set; }
}

public class PoiSearchRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadiusKm { get; set; } = 5.0;
    public List<string>? Categories { get; set; }
    public int MaxResults { get; set; } = 50;
}

public class PoiSearchResponse
{
    public List<Poi> Pois { get; set; } = new();
    public int TotalCount { get; set; }
    public double SearchRadiusKm { get; set; }
    public string SearchLocation { get; set; } = string.Empty;
} 