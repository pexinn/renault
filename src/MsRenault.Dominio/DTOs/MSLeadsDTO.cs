using System;
using System.Collections.Generic;
using MsRenault.Dominio.Agregadores;

namespace MsRenault.Dominio.Dtos
{
    public class MSLeadsDTO
    {
        public string CodigoLead { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }        
        public string? Telefone { get; set; } = string.Empty;
        public string Veiculo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string DocFaturado { get; set; } = string.Empty;
        public string NomeVendedor { get; set; } = string.Empty;
        public string DocVendedor { get; set; } = string.Empty;
        public string DescricaoLoja { get; set; } = string.Empty;
        public DateTime DataMensagem { get; set; }
        public DateTime? DataPrimeiraInteracao { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public string Midia { get; set; } = string.Empty;
        public string SistemaOrigem { get; set; } = string.Empty;
        public string OrigemLead { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public List<StatusLead> Status { get; set; } = [];
        public List<VisitaLead> Visitas { get; set; } = [];
        public List<LigacaoLead> Ligacoes { get; set; } = [];
        public List<TestDriveLead> TestDrives { get; set; } = [];
        public List<ComentarioLead> Comentarios { get; set; } = [];
        public List<TemperaturaLead> Temperaturas { get; set; } = [];
    }

    public class EventoRequisicaoDto
    {
        public string? Evento { get; set; }
        public MensagemAtualizacaoLead? Mensagem { get; set; }
        public string? Identificador { get; set; }
        public int IndexInteracoes { get; set; }
    }

    public class MensagemAtualizacaoLead
    {
        public MSLeadsDTO? Lead { get; set; }
        public string? Cnpj { get; set; }
    }

    public class EventoRequisicaoRespostaDto
    {
        public object? Mensagem { get; set; }
        public bool? Error { get; set; }
        public string? Identificador { get; set; }
        public string? Evento { get; set; }
    }
}
