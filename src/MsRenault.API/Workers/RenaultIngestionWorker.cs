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
        var renaultServico = scope.ServiceProvider.GetRequiredService<IRenaultServico>();

        _logger.LogInformation("Renault Ingestion Cycle started for BIR {Bir}", bir);

        await renaultServico.ObterLeads();
        
        _logger.LogInformation("Renault Ingestion Cycle completed for BIR {Bir}", bir);
    }
}
