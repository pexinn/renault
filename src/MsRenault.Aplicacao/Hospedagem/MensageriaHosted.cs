using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MsRenault.Aplicacao.Configuracao;
using MsRenault.Dominio.Interfaces;
using MsRenault.Infra.Mensageria.Services;

namespace MsRenault.Aplicacao.Hospedagem;

public class MensageriaHosted(
    IOptions<FilasConfiguracao> filasOptions,
    IRabbitMqService rabbitMqService,
    IRenaultServico renaultServico,
    IMessageProcessingService messageProcessingService
) : BackgroundService
{
    private readonly FilasConfiguracao _filasOptions = filasOptions.Value;
    private readonly IRabbitMqService _rabbitMqService = rabbitMqService;
    private readonly IRenaultServico _renaultServico = renaultServico;
    private readonly IMessageProcessingService _messageProcessingService = messageProcessingService;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // One consumer for Renault updates from MicroserviceLeads.Core (EventoMontadoraDTO)
        await _rabbitMqService.ConsumirMensagens(_renaultServico.ProcessarSolicitacao, _filasOptions.RenaultAtualizarLeadEnvio, "Renault");
        
        // Another consumer for generic process requests (similar to MSHyundai)
        await _rabbitMqService.ConsumirMensagens(_messageProcessingService.ExecuteAsync, _filasOptions.ProcessRenault, "Leads");
    }
}
