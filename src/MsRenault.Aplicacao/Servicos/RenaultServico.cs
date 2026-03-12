using MsRenault.Aplicacao.Configuracao;
using MsRenault.Dominio.Dtos;
using MsRenault.Dominio.Interfaces;
using MsRenault.Dominio.DTOs.Renault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace MsRenault.Aplicacao.Servicos;

public class RenaultServico(
    IOptions<FilasConfiguracao> filasOptions,
    IRenaultApiService renaultClient,
    IRabbitMqService rabbitMqService,
    IConfiguration configuration
    ) : IRenaultServico
{
    private readonly IRenaultApiService _renaultClient = renaultClient;
    private readonly IRabbitMqService _rabbitMqService = rabbitMqService;
    private readonly FilasConfiguracao _filasConfiguracao = filasOptions.Value;
    private readonly IConfiguration _configuration = configuration;

    public async Task ObterLeads()
    {
        try
        {
            var bir = _configuration["Renault:Bir"] ?? throw new Exception("BIR não configurado.");
            
            // Renault requirement: example 2 days range
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-2);

            var response = await _renaultClient.ConsumeLeadsAsync(bir, startDate, endDate);

            if (response.Data != null && response.Data.Any())
            {
                foreach (var leadData in response.Data)
                {
                    var msLead = MapToMSLeadsDTO(leadData);
                    await _rabbitMqService.PublishLeadAsync(_filasConfiguracao.Leads, msLead, msLead.CodigoLead);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao obter leads Renault: {ex.Message}");
        }
    }

    public async Task ProcessarSolicitacao(object mensagem)
    {
        try
        {
            var json = mensagem.ToString();
            if (string.IsNullOrEmpty(json)) return;

            // Note: MicroserviceLeads.Core sends EventoMontadoraDTO
            // We need to map this to Renault API calls
            // This is a simplified version, real implementation would inspect the event type
            
            // For now, let's assume we logic similar to MSHyundai or direct call to Update Funnel
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao processar solicitação Renault: {ex.Message}");
        }
    }

    public async Task<object> AtualizarLead(MSLeadsDTO lead, string cnpj, int indexInteracoes)
    {
        // Implementation to call Renault API based on Lead update
        // Example: Update Funnel or Prospection
        return new { Success = true };
    }

    private MSLeadsDTO MapToMSLeadsDTO(RenaultLeadData lead)
    {
        return new MSLeadsDTO
        {
            CodigoLead = lead.LeadReferenceId,
            Nome = $"{lead.Client.FirstName} {lead.Client.LastName}".Trim(),
            Email = lead.Client.Email,
            Telefone = lead.Client.MobilePhone,
            Veiculo = lead.Vehicle.ModelOfInterest,
            DataCriacao = DateTime.Parse(lead.SubmissionTimestamp),
            DataMensagem = DateTime.UtcNow,
            SistemaOrigem = "MSRenault",
            OrigemLead = "Montadora",
            Cnpj = _configuration["Renault:LojaCnpj"] ?? string.Empty // Need to map BIR to CNPJ if possible
        };
    }
}
