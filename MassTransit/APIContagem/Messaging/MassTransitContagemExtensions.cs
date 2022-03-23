using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using MassTransit.KafkaIntegration;

namespace APIContagem.Messaging
{
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
                    bus.UsingAzureServiceBus((context, cfg) =>
                    {
                        cfg.Host(configuration["AzureServiceBus:ConnectionString"]);
                        cfg.Message<ResultadoContadorContract>(configTopology =>
                        {
                            configTopology.SetEntityName(
                                configuration["AzureServiceBus:Topic"]);
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
                        rider.AddProducer<ResultadoContadorContract>(configuration["ApacheKafka:Topic"]);

                        rider.UsingKafka(
                            new ()
                            {
                                SecurityProtocol = Confluent.Kafka.SecurityProtocol.SaslSsl,
                                SaslMechanism = Confluent.Kafka.SaslMechanism.Plain,
                                SaslUsername = configuration["ApacheKafka:Username"],
                                SaslPassword = configuration["ApacheKafka:Password"]
                            },
                            (_, kafka) =>
                            {
                                kafka.Host(configuration["ApacheKafka:Host"]);
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
                        rider.AddProducer<ResultadoContadorContract>(configuration["AzureEventHubs:EventHub"]);

                        rider.UsingEventHub((_, eventHubs) =>
                        {
                            eventHubs.Host(configuration["AzureEventHubs:ConnectionString"]);
                            eventHubs.Storage(configuration["AzureEventHubs:AzureStorage"]);
                        });
                    });
                });
    }
}