using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Net;
using System.Net.Sockets;

namespace MainApplication
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    public class TcpListenerProbe : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly TcpListener _listener;
        private readonly HealthCheckService _healthCheckService;


        public TcpListenerProbe(
            ILogger<Worker> logger,
            HealthCheckService healthCheckService)
        {
            _logger = logger;
            _healthCheckService = healthCheckService;

            var port = Convert.ToInt16(Environment.GetEnvironmentVariable("TCP_PORT") ?? "5000");

            _listener = new TcpListener(IPAddress.Any, port);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _listener.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                await UpdateHeartbeat(stoppingToken);

                await Task.Delay(10000, stoppingToken);
            }
        }

        private async Task UpdateHeartbeat(CancellationToken token)
        {
            try
            {
                var result = await _healthCheckService.CheckHealthAsync(token);
                var isHealthy = result.Status == HealthStatus.Healthy;

                if (!isHealthy)
                {
                    _logger.LogWarning("Service is unhealthy!!!");

                    await WritingResult(result);

                    return;
                }

                await WritingResult(result);

                while (_listener.Server.IsBound && _listener.Pending())
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    client.Close();


                    _logger.LogInformation("Service is healthy.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Occurred error while executing heartbeat process");
            }
        }

        private async Task WritingResult(HealthReport result)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };


            using (StreamWriter escritor = new StreamWriter("logs/health-check.json"))
            {
                string conteudo = JsonConvert.SerializeObject(result, settings);

                escritor.WriteLine(conteudo);
            }
        }
    }
}