namespace MsRenault.Dominio.Interfaces;

public interface IRabbitMqService
{
    Task PublishLeadAsync<T>(string queueName, T message, string correlationId, CancellationToken ct = default);
}
