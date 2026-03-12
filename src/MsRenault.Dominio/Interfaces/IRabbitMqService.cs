using System.Threading.Tasks;
using MsRenault.Dominio.Dtos;

namespace MsRenault.Dominio.Interfaces;

public interface IRabbitMqService
{
    Task PublishLeadAsync<T>(string queueName, T message, string correlationId, CancellationToken ct = default);
    Task ConsumirMensagens(Func<string, Task> processarMensagem, string fila, string vhost = "");
}
