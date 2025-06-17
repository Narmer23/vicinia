using System.ComponentModel.DataAnnotations;

namespace Vicinia.HistoryService.Models;

public class SearchHistory
{
    public Guid Id { get; set; }
    
    public Guid? UserId { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    public double Latitude { get; set; }
    
    [Required]
    public double Longitude { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string TransportationMode { get; set; } = string.Empty;
    
    [Required]
    public double OverallScore { get; set; }
    
    [Required]
    public DateTime SearchDate { get; set; }
    
    [Required]
    public int PoiCount { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
} 