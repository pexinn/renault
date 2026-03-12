using Microsoft.Extensions.Options;
using MsRenault.Aplicacao.Configuracao;
using MsRenault.Dominio.Dtos;
using MsRenault.Dominio.Interfaces;
using Newtonsoft.Json;

namespace MsRenault.Infra.Mensageria.Services;

public class MessageProcessingService(
    IRabbitMqService _rabbitMqService,
    IRenaultServico _renaultService,
    IOptions<FilasConfiguracao> _filasConfiguracao
) : IMessageProcessingService
{
    public async Task ExecuteAsync(object mensagem)
    {
        var json = mensagem as string ?? "";
        var requisicao = JsonConvert.DeserializeObject<EventoRequisicaoDto>(json)
                         ?? throw new InvalidOperationException("Mensagem inválida para atualização de lead Renault.");

        try
        {
            var lead = requisicao.Mensagem?.Lead
                       ?? throw new InvalidOperationException("Lead não encontrado na mensagem de atualização.");

            var response = await _renaultService.AtualizarLead(lead, lead.Cnpj, requisicao.IndexInteracoes);

            await EnviarResposta(response, requisicao, false);
        }
        catch (Exception ex)
        {
            await EnviarResposta(ex.Message, requisicao, true);
            throw;
        }
    }

    private async Task EnviarResposta(object? response, EventoRequisicaoDto requisicao, bool error)
    {
        if (response == null) return;

        var resposta = new EventoRequisicaoRespostaDto
        {
            Mensagem = response,
            Error = error,
            Identificador = requisicao.Identificador,
            Evento = requisicao.Evento
        };

        await _rabbitMqService.PublishLeadAsync(_filasConfiguracao.Value.RenaultAtualizarLeadRetorno, resposta, requisicao.Identificador ?? Guid.NewGuid().ToString());
    }
}
