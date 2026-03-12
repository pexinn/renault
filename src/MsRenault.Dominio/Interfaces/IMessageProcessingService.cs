using System.Threading.Tasks;
using MsRenault.Dominio.Dtos;

namespace MsRenault.Dominio.Interfaces;

public interface IMessageProcessingService
{
    Task ExecuteAsync(object mensagem);
}
