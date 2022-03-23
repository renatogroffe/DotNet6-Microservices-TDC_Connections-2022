using System.Text.Json;
using MassTransit;
using APIContagem.Messaging;

namespace WorkerConsumerContagem.Messaging;

public class ContadorConsumer : IConsumer<ResultadoContadorContract>
{
    private readonly ILogger<ContadorConsumer> _logger;

    public ContadorConsumer(ILogger<ContadorConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ResultadoContadorContract> context)
    {
        var resultadoContador = context.Message;
        _logger.LogInformation($"Valor de contagem recebido = {resultadoContador.ValorAtual}");
        _logger.LogInformation(JsonSerializer.Serialize(resultadoContador));
        return Task.CompletedTask;
    }
}