using MainApplication;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHealthChecks()
        .AddCheck<ApiHealthCheck>("api");
        services.AddHostedService<Worker>();
        services.AddHostedService<TcpListenerProbe>();

    })
    .Build();

await host.RunAsync();
