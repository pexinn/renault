using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MsRenault.Dominio.Interfaces;
using MsRenault.Dominio.Entities;
using System.Text.Json;
using System.Diagnostics;

namespace MsRenault.API.Workers;

public class RenaultIngestionWorker : BackgroundService
{
    private readonly ILogger<RenaultIngestionWorker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ActivitySource _activitySource = new("MsRenault.Ingestion");

    public RenaultIngestionWorker(
        ILogger<RenaultIngestionWorker> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMinutes = _configuration.GetValue<int>("Ingestion:IntervalMinutes", 15);
        var bir = _configuration["Renault:Bir"] ?? "UNKNOWN";

        _logger.LogInformation("Renault Ingestion Worker starting with interval of {Interval} minutes for BIR {Bir}", intervalMinutes, bir);

        while (!stoppingToken.IsCancellationRequested)
        {
            using var activity = _activitySource.StartActivity("IngestionCycle");
            try
            {
                await ProcessIngestionAsync(bir, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Renault ingestion cycle");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            }

            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }
    }

    private async Task ProcessIngestionAsync(string bir, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var apiService = scope.ServiceProvider.GetRequiredService<IRenaultApiService>();
        var repository = scope.ServiceProvider.GetRequiredService<ILeadRepository>();
        var rabbitMq = scope.ServiceProvider.GetRequiredService<IRabbitMqService>();

        // Renault requirement: max 2 days range.
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-2);

        _logger.LogInformation("Consuming leads from {Start} to {End}", startDate, endDate);

        var response = await apiService.ConsumeLeadsAsync(bir, startDate, endDate, ct);

        if (response.Data.Count == 0)
        {
            _logger.LogInformation("No new leads found for BIR {Bir}", bir);
            return;
        }

        foreach (var lead in response.Data)
        {
            // US02: Save raw payload to MongoDB
            var rawLead = new RawRenaultLead
            {
                LeadReferenceId = lead.LeadReferenceId,
                Bir = bir,
                RawJson = JsonSerializer.Serialize(lead),
                Status = "Received"
            };

            await repository.CreateAsync(rawLead, ct);

            // US03: Publish to RabbitMQ
            await rabbitMq.PublishLeadAsync("renault.leads.received", lead, lead.LeadReferenceId, ct);
            
            _logger.LogInformation("Lead {LeadId} processed and published", lead.LeadReferenceId);
        }
    }
}
