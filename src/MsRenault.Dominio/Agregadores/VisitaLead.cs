using System;

namespace MsRenault.Dominio.Agregadores;

public class VisitaLead
{
    public string? Titulo { get; set; }
    public string? Descricao { get; set; }
    public DateTime DataHora { get; set; }
    public DateTime DataAgendamento { get; set; }
    public DateTime? DataRealizado { get; set; }
    public string? Status { get; set; }
}
