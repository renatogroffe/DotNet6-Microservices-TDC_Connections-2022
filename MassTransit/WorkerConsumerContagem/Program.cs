using WorkerConsumerContagem;
using MassTransit;
using WorkerConsumerContagem.Messaging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddMassTransitContagem(hostContext.Configuration);
        services.AddMassTransitHostedService();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
