using System;

namespace MsRenault.Dominio.Agregadores;

public class StatusLead
{
    public string? Status { get; set; }
    public DateTime DataHora { get; set; }
    public string? Motivo { get; set; }
    public string? SubMotivo { get; set; }
}
