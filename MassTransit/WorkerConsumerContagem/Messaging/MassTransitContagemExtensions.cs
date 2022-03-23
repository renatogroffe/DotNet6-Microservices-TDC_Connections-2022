using MassTransit;
using APIContagem.Messaging;

namespace WorkerConsumerContagem.Messaging;

public static class MassTransitContagemExtensions
{
    public static IServiceCollection AddMassTransitContagem(
        this IServiceCollection services,
        IConfiguration configuration) => configuration["MassTransitTechnology"] switch
        {
            "AzureServiceBus" => services.AddMassTransitContagemServiceBus(configuration),
            "ApacheKafka" => services.AddMassTransitContagemKafka(configuration),
            "AzureEventHubs" => services.AddMassTransitContagemEventHubs(configuration),
            _ => throw new InvalidOperationException(
                            "Não foi definida a tecnologia para se trabalhar com o MassTransit!")
        };

    private static IServiceCollection AddMassTransitContagemServiceBus(
        this IServiceCollection services,
        IConfiguration configuration) =>
            services.AddMassTransit(bus =>
            {
                bus.AddConsumer<ContadorConsumer>();
                bus.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(configuration["AzureServiceBus:ConnectionString"]);

                    cfg.SubscriptionEndpoint(
                        configuration["AzureServiceBus:Subscription"],
                        configuration["AzureServiceBus:Topic"],
                        configEndpoint =>
                        {
                            configEndpoint.Consumer<ContadorConsumer>(context);
                        });

                    cfg.ConfigureEndpoints(context);
                });
            });

    private static IServiceCollection AddMassTransitContagemKafka(
        this IServiceCollection services,
        IConfiguration configuration) =>
            services.AddMassTransit(bus =>
            {
                bus.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(configuration["ApacheKafka:AzureServiceBus"]);
                    cfg.ConfigureEndpoints(context);
                });

                bus.AddRider(rider =>
                {
                    rider.AddConsumer<ContadorConsumer>();

                    rider.UsingKafka(
                        new()
                        {
                            SecurityProtocol = Confluent.Kafka.SecurityProtocol.SaslSsl,
                            SaslMechanism = Confluent.Kafka.SaslMechanism.Plain,
                            SaslUsername = configuration["ApacheKafka:Username"],
                            SaslPassword = configuration["ApacheKafka:Password"]
                        },
                        (context, kafka) =>
                        {
                            kafka.Host(configuration["ApacheKafka:Host"]);

                            kafka.TopicEndpoint<ResultadoContadorContract>(
                                configuration["ApacheKafka:Topic"],
                                configuration["ApacheKafka:GroupId"],
                                endpoint =>
                                {
                                    endpoint.ConfigureConsumer<ContadorConsumer>(context);
                                }
                            );
                        });
                });
            });

    private static IServiceCollection AddMassTransitContagemEventHubs(
        this IServiceCollection services,
        IConfiguration configuration) =>
            services.AddMassTransit(bus =>
            {
                bus.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(configuration["AzureEventHubs:AzureServiceBus"]);
                    cfg.ConfigureEndpoints(context);
                });

                bus.AddRider(rider =>
                {
                    rider.AddConsumer<ContadorConsumer>();

                    rider.UsingEventHub((context, eventHubs) =>
                    {
                        eventHubs.Host(configuration["AzureEventHubs:ConnectionString"]);
                        eventHubs.Storage(configuration["AzureEventHubs:AzureStorage"]);
                        eventHubs.ReceiveEndpoint(
                            configuration["AzureEventHubs:EventHub"],
                            configuration["AzureEventHubs:ConsumerGroup"],
                            endpoint =>
                            {
                                endpoint.ConfigureConsumer<ContadorConsumer>(context);
                            }
                        );
                    });
                });
            });
}