using System.Net.Sockets;

namespace HeartBeat
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private const string IP = "127.0.0.1";
        private const int PORT = 5000;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                TcpClient client = new TcpClient();
                string result = string.Empty;

                try
                {
                    client.Connect(IP, PORT);

                    result = File.ReadAllText("logs/health-check.json").ToLower();

                    if (result.Contains("unhealthy"))
                        _logger.LogError($"Heartbeat check ISN'T working properly. The result was inserted in the log tool centralized\n\n {result}");
                    else
                        _logger.LogInformation($"Heartbeat check IS working properly. The result was inserted in the log tool centralized\n\n {result}");
                }
                catch (Exception ex)
                {
                    _logger.LogCritical("Heartbeat check isn't working. Your main application is down", ex);
                }
                finally
                {
                    client.Close();
                }

                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}