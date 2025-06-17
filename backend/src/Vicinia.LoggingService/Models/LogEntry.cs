using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Vicinia.LoggingService.Models;

public class LogEntry
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string ServiceName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string LogLevel { get; set; } = string.Empty;
    
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public string? Exception { get; set; }
    
    [Required]
    public DateTime Timestamp { get; set; }
    
    public Guid? UserId { get; set; }
    
    [MaxLength(100)]
    public string? RequestId { get; set; }
    
    public JsonDocument? AdditionalData { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
} 