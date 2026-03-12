using Microsoft.AspNetCore.Mvc;
using MsRenault.Dominio.Interfaces;
using MsRenault.Dominio.DTOs.Salesforce;
using System.Diagnostics;

namespace MsRenault.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeadController : ControllerBase
{
    private readonly IRabbitMqService _rabbitMqService;
    private readonly ILogger<LeadController> _logger;
    private readonly ActivitySource _activitySource = new("MsRenault.API");

    public LeadController(IRabbitMqService rabbitMqService, ILogger<LeadController> logger)
    {
        _rabbitMqService = rabbitMqService;
        _logger = logger;
    }

    [HttpPost("webhook/salesforce/event")]
    public async Task<IActionResult> ReceiveSalesforceEvent([FromBody] SalesforceLead leadEvent)
    {
        using var activity = _activitySource.StartActivity("ReceiveSalesforceEvent");
        
        _logger.LogInformation("Received event from Salesforce for Lead {LeadId}", leadEvent.Id);

        // US06: Publish to outbound funnel queue
        await _rabbitMqService.PublishLeadAsync("renault.outbound.events", leadEvent, leadEvent.Id ?? Guid.NewGuid().ToString());

        return Accepted();
    }
}
