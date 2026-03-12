namespace MsRenault.Aplicacao.Dtos;

public class SecretsDTO
{
    public required string NomeSecret { get; set; }
    public required string VersaoSecret { get; set; }
    public BancoDTO? ConfigBanco { get; set; }
}
