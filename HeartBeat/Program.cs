using HeartBeat;


public class Program
{
    private static IHostEnvironment _env;

    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                _env = hostingContext.HostingEnvironment;
                Console.WriteLine($"Hosting environment: {_env.EnvironmentName} \n\n");
                config.AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile($"appsettings.{_env.EnvironmentName}.json", true, true);
                config.AddEnvironmentVariables();
            }).ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>();


            }).ConfigureLogging((hostingContext, logging) =>
            {
            });
    }
}