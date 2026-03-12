using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using MsRenault.Dominio.Interfaces;
using System.Diagnostics;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;

namespace MsRenault.Infra.Mensageria.Services;

public class RabbitMqService : IRabbitMqService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqService> _logger;
    private readonly TextMapPropagator _propagator = Propagators.DefaultTextMapPropagator;
    private readonly ActivitySource _activitySource = new("MsRenault.Messaging");

    public RabbitMqService(IConfiguration configuration, ILogger<RabbitMqService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task PublishLeadAsync<T>(string queueName, T message, string correlationId, CancellationToken ct = default)
    {
        using var activity = _activitySource.StartActivity($"Publish {queueName}", ActivityKind.Producer);
        
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
            UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest"
        };

        using var connection = await factory.CreateConnectionAsync(ct);
        using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var props = new BasicProperties
        {
            CorrelationId = correlationId,
            Persistent = true,
            Headers = new Dictionary<string, object?>()
        };

        // OpenTelemetry Context Injection
        var context = activity?.Context ?? Activity.Current?.Context ?? default;
        _propagator.Inject(new PropagationContext(context, Baggage.Current), props.Headers, (headers, key, value) =>
        {
            headers[key] = value;
        });

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queueName,
            mandatory: true,
            basicProperties: props,
            body: body,
            cancellationToken: ct);

        _logger.LogInformation("Message published to {Queue} with CorrelationId {CorrelationId}", queueName, correlationId);
    }
}
