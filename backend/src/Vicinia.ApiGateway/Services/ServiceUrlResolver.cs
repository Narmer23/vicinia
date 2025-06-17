using Microsoft.Extensions.Configuration;

namespace Vicinia.ApiGateway.Services
{
    public interface IServiceUrlResolver
    {
        string GetServiceUrl(string serviceName);
    }

    public class ServiceUrlResolver : IServiceUrlResolver
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public ServiceUrlResolver(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public string GetServiceUrl(string serviceName)
        {
            var environment = _environment.IsDevelopment() ? "Development" : "Docker";
            var serviceConfig = _configuration.GetSection($"Services:{serviceName}");
            
            if (serviceConfig.Exists())
            {
                var url = serviceConfig[environment];
                if (!string.IsNullOrEmpty(url))
                {
                    return url;
                }
            }

            // Fallback for services not in configuration
            return environment == "Development" 
                ? $"http://localhost:{GetDefaultPort(serviceName)}"
                : $"http://{serviceName.ToLower()}-service:80";
        }

        private static int GetDefaultPort(string serviceName)
        {
            return serviceName.ToLower() switch
            {
                "userservice" => 5002,
                "scoringservice" => 5003,
                "geocodingservice" => 5004,
                "poiservice" => 5005,
                "historyservice" => 5006,
                "loggingservice" => 5007,
                _ => 5000
            };
        }
    }
} 