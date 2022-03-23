namespace APIContagem.Messaging;

public interface ResultadoContadorContract
{
    public int ValorAtual { get; }
    public string? Versao { get; set; }
    public string? Local { get; }
    public string? Kernel { get; }
    public string? Framework { get; }
    public string? Mensagem { get; }
}