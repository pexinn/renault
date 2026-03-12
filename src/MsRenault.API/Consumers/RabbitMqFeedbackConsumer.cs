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
using MsRenault.Dominio.DTOs.Salesforce;
using MsRenault.Dominio.Constants;
using System.Diagnostics;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;

namespace MsRenault.API.Consumers;

public class RabbitMqFeedbackConsumer : BackgroundService
{
    private readonly ILogger<RabbitMqFeedbackConsumer> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ActivitySource _activitySource = new("MsRenault.FeedbackConsumer");

    public RabbitMqFeedbackConsumer(
        ILogger<RabbitMqFeedbackConsumer> logger,
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

        var queueName = "renault.outbound.events";
        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            using var activity = _activitySource.StartActivity("ProcessFeedbackMessage", ActivityKind.Consumer);
            
            try
            {
                var body = ea.Body.ToArray();
                var leadEvent = JsonSerializer.Deserialize<SalesforceLead>(Encoding.UTF8.GetString(body));

                if (leadEvent != null)
                {
                    await ProcessFeedbackAsync(leadEvent, stoppingToken);
                }

                await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing feedback message {DeliveryTag}", ea.DeliveryTag);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                await channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(queueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        _logger.LogInformation("RabbitMqFeedbackConsumer starting. Listening to {Queue}", queueName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessFeedbackAsync(SalesforceLead lead, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var renaultApi = scope.ServiceProvider.GetRequiredService<IRenaultApiService>();

        // Logic to determine which Renault endpoint to call based on Lead details
        // For demonstration, we use the status change logic (US06)
        
        if (!string.IsNullOrEmpty(lead.RenaultLeadReferenceId))
        {
            // US06: Funnel Update
            var funnelRequest = lead.ToRenaultFunnelRequest();
            await renaultApi.UpdateFunnelAsync(lead.RenaultLeadReferenceId, funnelRequest, ct);
            _logger.LogInformation("Funnel updated for Lead {LeadId}", lead.RenaultLeadReferenceId);

            // US07: Prospection (if status is PROSPECTION)
            if (lead.Status == RenaultConstants.Status.Prospection)
            {
                var prospectionRequest = lead.ToRenaultProspectionRequest("Attendant", "SalesPerson", true);
                await renaultApi.UpdateProspectionAsync(lead.RenaultLeadReferenceId, prospectionRequest, ct);
                _logger.LogInformation("Prospection updated for Lead {LeadId}", lead.RenaultLeadReferenceId);
            }
        }
        else
        {
            // US09: Exclusive Lead Creation (Phase 3)
            var bir = _configuration["Renault:Bir"] ?? "UNKNOWN";
            var createRequest = lead.ToRenaultCreateRequest();
            var newRefId = await renaultApi.CreateLeadAsync(bir, createRequest, ct);
            _logger.LogInformation("Exclusive Lead created in Renault. New RefId: {RefId}", newRefId);
            
            // In a real scenario, we would sync this back to Salesforce and MongoDB
        }
    }
}
