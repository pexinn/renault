using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MsRenault.Dominio.DTOs.Salesforce;
using MsRenault.Dominio.Interfaces;

namespace MsRenault.Infra.Dados.Services;

public class SalesforceApiService : ISalesforceApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SalesforceApiService> _logger;

    public SalesforceApiService(HttpClient httpClient, IConfiguration configuration, ILogger<SalesforceApiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> CreateLeadAsync(SalesforceLead lead, CancellationToken ct = default)
    {
        _logger.LogInformation("Sending lead {FirstName} {LastName} to Salesforce...", lead.FirstName, lead.LastName);

        var url = _configuration["Salesforce:BaseUrl"] ?? "https://na1.salesforce.com/services/data/v60.0/sobjects/Lead";
        
        // In a real scenario, we would handle Salesforce OAuth2 here or use a dedicated service.
        // Assuming the HttpClient is pre-configured with the token or we add it here.
        
        var response = await _httpClient.PostAsJsonAsync(url, lead, ct);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<SalesforceResponse>(cancellationToken: ct);
            return result?.Id ?? "MOCK_SF_ID_" + Guid.NewGuid().ToString("N");
        }

        var error = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Failed to create lead in Salesforce. Status: {Status}, Error: {Error}", response.StatusCode, error);
        
        throw new HttpRequestException($"Salesforce API error: {response.StatusCode} - {error}");
    }

    private record SalesforceResponse(string Id, bool Success);
}
