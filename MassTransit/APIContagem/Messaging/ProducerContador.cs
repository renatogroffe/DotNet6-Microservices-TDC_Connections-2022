using MassTransit;
using MassTransit.KafkaIntegration;
using MassTransit.EventHubIntegration;

namespace APIContagem.Messaging;

public class ProducerContador
{
    private IPublishEndpoint? _publishEndpoint = null;
    private IEventHubProducer? _eventHubProducer = null;
    private ITopicProducer<ResultadoContadorContract>? _topicProducerKafka = null;

    public ProducerContador(
        IConfiguration configuration, HttpContext context)
    {
        switch (configuration["MassTransitTechnology"])
        {
            case "AzureServiceBus":
                _publishEndpoint =
                    context.RequestServices.GetRequiredService<IPublishEndpoint>();
                break;
            case "ApacheKafka":
                _topicProducerKafka =
                    context.RequestServices.GetRequiredService<ITopicProducer<ResultadoContadorContract>>();
                break;
            case "AzureEventHubs":
                var producerProvider =
                    context.RequestServices.GetRequiredService<IEventHubProducerProvider>();
                _eventHubProducer = producerProvider.GetProducer(
                    configuration["AzureEventHubs:EventHub"]).Result;
                break;
            default:
                throw new InvalidOperationException(
                    "Não foi definida a tecnologia para se trabalhar com um "+
                    "Producer/Publisher do MassTransit!");
        }
    }

    public async Task Produce(ResultadoContadorContract resultado)
    {
        if (_publishEndpoint is not null)
            await _publishEndpoint.Publish<ResultadoContadorContract>(resultado);
        else if (_topicProducerKafka is not null)
            await _topicProducerKafka.Produce(resultado);
        else if (_eventHubProducer is not null)
            await _eventHubProducer.Produce<ResultadoContadorContract>(resultado);
    }
}