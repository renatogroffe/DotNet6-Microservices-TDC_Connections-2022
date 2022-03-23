using APIContagem.Messaging;

namespace APIContagem.V2.Models;

public class ResultadoContador : ResultadoContadorContract
{
    public int ValorAtual { get; set; }
    public string? Versao { get; set; }
    public string? Local { get; set; }
    public string? Kernel { get; set; }
    public string? Framework { get; set; }
    public string? Mensagem { get; set; }
}