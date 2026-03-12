using System.Threading.Tasks;
using MsRenault.Dominio.Dtos;

namespace MsRenault.Dominio.Interfaces;

public interface IRenaultServico
{
    Task ObterLeads();
    Task ProcessarSolicitacao(object mensagem);
    Task<object> AtualizarLead(MSLeadsDTO lead, string cnpj, int indexInteracoes);
}
