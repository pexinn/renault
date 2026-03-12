using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MsRenault.Dominio.Interfaces;
using MsRenault.Dominio.Mappers;
using MsRenault.Dominio.DTOs.Renault;
using System.Diagnostics;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;

namespace MsRenault.API.Consumers;

public class RabbitMqLeadConsumer : BackgroundService
{
    private readonly ILogger<RabbitMqLeadConsumer> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly TextMapPropagator _propagator = Propagators.DefaultTextMapPropagator;
    private readonly ActivitySource _activitySource = new("MsRenault.Consumer");

    public RabbitMqLeadConsumer(
        ILogger<RabbitMqLeadConsumer> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
            UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest"
        };

        using var connection = await factory.CreateConnectionAsync(stoppingToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        var queueName = "renault.leads.received";
        var dlqName = "renault.leads.dlq";

        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await channel.QueueDeclareAsync(dlqName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            // Extract OTel Context
            var parentContext = _propagator.Extract(default, ea.BasicProperties.Headers, (headers, key) =>
            {
                if (headers != null && headers.TryGetValue(key, out var value) && value is byte[] bytes)
                {
                    return new[] { Encoding.UTF8.GetString(bytes) };
                }
                return Enumerable.Empty<string>();
            });

            using var activity = _activitySource.StartActivity("ProcessLeadMessage", ActivityKind.Consumer, parentContext.ActivityContext);
            
            try
            {
                var body = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<RenaultLeadData>(Encoding.UTF8.GetString(body));

                if (message != null)
                {
                    await ProcessMessageAsync(message, stoppingToken);
                }

                await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing lead message {DeliveryTag}", ea.DeliveryTag);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                // Publish to DLQ
                var props = new BasicProperties
                {
                    CorrelationId = ea.BasicProperties.CorrelationId,
                    Persistent = true
                };
                await channel.BasicPublishAsync(string.Empty, dlqName, true, props, ea.Body, stoppingToken);
                await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(queueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        _logger.LogInformation("RabbitMqLeadConsumer starting. Listening to {Queue}", queueName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessMessageAsync(RenaultLeadData lead, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var salesforceApi = scope.ServiceProvider.GetRequiredService<ISalesforceApiService>();
        var repository = scope.ServiceProvider.GetRequiredService<ILeadRepository>();

        _logger.LogInformation("Processing lead {LeadId} for Salesforce integration", lead.LeadReferenceId);

        // US04: Map and Send to Salesforce
        var sfLead = lead.ToSalesforceLead();
        var sfId = await salesforceApi.CreateLeadAsync(sfLead, ct);

        // Update MongoDB status
        await repository.UpdateStatusAsync(lead.LeadReferenceId, "SentToSalesforce", sfId, ct);

        _logger.LogInformation("Lead {LeadId} successfully integrated with Salesforce (ID: {SfId})", lead.LeadReferenceId, sfId);
    }
}
