using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainApplication
{
    public class ApiHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        public ApiHealthCheck()
        {
            _httpClient = new HttpClient();
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                
                var result = await Get<dynamic>("http://<your-api>:<port>/healthz");

                if (result != null)
                    return new HealthCheckResult(HealthStatus.Healthy);

                return new HealthCheckResult(HealthStatus.Unhealthy);
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, description: $"Error BateVolta API - {ex.Message}");
            }

        }

        public async Task<T> Get<T>(string uri)
        {
            var response = await _httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            var conteudo = await response.Content.ReadAsStringAsync();

            return await Task.Run(() => JsonConvert.DeserializeObject<T>(conteudo));
        }
    }
}
