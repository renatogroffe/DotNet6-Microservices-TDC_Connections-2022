namespace WorkerConsumerContagem;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly int _intervaloMensagemWorkerAtivo;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        logger.LogInformation(
            $"Tecnologia definida para uso do MassTransit: {configuration["MassTransitTechnology"]}");

        _logger = logger;
        _intervaloMensagemWorkerAtivo =
            Convert.ToInt32(configuration["IntervaloMensagemWorkerAtivo"]);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                $"Worker ativo em: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            await Task.Delay(_intervaloMensagemWorkerAtivo, stoppingToken);
        }
    }
}