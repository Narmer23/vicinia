using System.Text.Json;
using Vicinia.ScoringService.Models;

namespace Vicinia.ScoringService.Services;

public interface IHistoryServiceClient
{
    Task<bool> SaveSearchHistoryAsync(SearchHistoryRequest history);
}

public class HistoryServiceClient : IHistoryServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HistoryServiceClient> _logger;
    private readonly IServiceUrlResolver _serviceUrlResolver;

    public HistoryServiceClient(HttpClient httpClient, ILogger<HistoryServiceClient> logger, IServiceUrlResolver serviceUrlResolver)
    {
        _httpClient = httpClient;
        _logger = logger;
        _serviceUrlResolver = serviceUrlResolver;
    }

    public async Task<bool> SaveSearchHistoryAsync(SearchHistoryRequest history)
    {
        try
        {
            _logger.LogInformation("Saving search history for user: {UserId}", history.UserId);

            var historyServiceUrl = _serviceUrlResolver.GetServiceUrl("HistoryService");
            var json = JsonSerializer.Serialize(history);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{historyServiceUrl}/api/history", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully saved search history for user: {UserId}", history.UserId);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to save search history for user: {UserId}. Status: {StatusCode}", 
                    history.UserId, response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving search history for user: {UserId}", history.UserId);
            return false;
        }
    }
}

public class SearchHistoryRequest
{
    public Guid? UserId { get; set; }
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string TransportationMode { get; set; } = string.Empty;
    public double OverallScore { get; set; }
    public int PoiCount { get; set; }
} 